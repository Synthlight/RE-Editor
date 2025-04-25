using System.Diagnostics.CodeAnalysis;
using System.IO;

#pragma warning disable CS8618

namespace RE_Editor.Common.Models;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
public class RSZ {
    public long                  position;
    public uint                  magic; // We only support: 0x52535A00
    public uint                  version; // We only support: 16
    public uint                  reserved; // Might be padding?
    public List<uint>            objectEntryPoints; // Outermost data type.
    public List<InstanceInfo>    instanceInfo; // Array type info. (Contents of the 'outermost' data type, given the outermost type is an array.)
    public List<RszUserDataInfo> userDataInfo; // String names of other files being referenced. MUST MATCH THE OUTER ONE IN `ReDataFile`.
    public List<RszObject>       objectData; // Array data. USED ONLY FOR READ.

    public static RSZ Read(BinaryReader reader, bool justReadHashes) {
#pragma warning disable IDE0017
        var rsz = new RSZ();
#pragma warning restore IDE0017
        rsz.position = reader.BaseStream.Position;
        rsz.magic    = reader.ReadUInt32();
        rsz.version  = reader.ReadUInt32();
        var objectInfoCount = reader.ReadInt32();
        var instanceCount   = reader.ReadInt32();
        var userDataCount   = reader.ReadInt32();
        rsz.reserved = reader.ReadUInt32();
        var instanceOffset = reader.ReadUInt64();
        var dataOffset     = reader.ReadUInt64();
        var userDataOffset = reader.ReadUInt64();

        rsz.objectEntryPoints = new(objectInfoCount);
        for (var i = 0; i < objectInfoCount; i++) {
            rsz.objectEntryPoints.Add(reader.ReadUInt32());
        }

        reader.BaseStream.Seek(rsz.position + (long) instanceOffset, SeekOrigin.Begin);
        rsz.instanceInfo = new(instanceCount);
        for (var i = 0; i < instanceCount; i++) {
            rsz.instanceInfo.Add(InstanceInfo.Read(reader));
        }

        if (justReadHashes) return rsz;

        reader.BaseStream.Seek(rsz.position + (long) userDataOffset, SeekOrigin.Begin);
        rsz.userDataInfo = new(userDataCount);
        for (var i = 0; i < userDataCount; i++) {
            var userDataInfo = RszUserDataInfo.Read(reader, rsz);
            rsz.userDataInfo.Add(userDataInfo);
        }

        reader.BaseStream.Seek(rsz.position + (long) dataOffset, SeekOrigin.Begin);
        rsz.objectData = new(instanceCount);
        for (var index = 0; index < instanceCount; index++) {
            var hash = rsz.instanceInfo[index].hash;
            if (hash == 0) continue;
            var userDataRef = -1; // index - 1 due to the first instance info always being null.
            for (var i = 0; i < rsz.userDataInfo.Count; i++) {
                if (rsz.userDataInfo[i].instanceId == index) {
                    userDataRef = i;
                    break;
                }
            }
            var obj = RszObject.Read(reader, hash, rsz, userDataRef);
            obj.Index = index;
            rsz.objectData.Add(obj);
        }

        if (reader.BaseStream.Position != reader.BaseStream.Length) throw new("Finished reading the file but the position is not at the end.");

        return rsz;
    }

    [SuppressMessage("ReSharper", "ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator")]
    [SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
    public void Write(BinaryWriter writer, ulong rszOffsetStart, bool testWritePosition, bool forGp) {
        lock (this) {
            // We need to pre-calculate the instance info array before we write it.
            // This will also give us the entry object, but we're just not supporting adding/removing instances for now.
            instanceInfo.Clear();
            instanceInfo.Add(new() {hash = 0, crc = 0});

            // Re-create the `objectData` array in case we've added data.
            // First, get a list of entry objects.
            var rootObjects = new List<RszObject>();
            foreach (var entryIndex in objectEntryPoints) {
                var entryPointObject = objectData[(int) entryIndex - 1]; // 1 based.
                rootObjects.Add(entryPointObject);
            }
            // Then, call each in order, so they can rebuild the list in the right order.
            objectData.Clear();
            foreach (var rootObject in rootObjects) {
                var objectChain = new List<RszObject>();
                // Build the object data chain.
                rootObject.WriteObjectList(objectChain);
                objectData.AddRange(objectChain);
                // And build the instance info chain.
                rootObject.SetupInstanceInfo(instanceInfo, forGp);
            }
            // Finally, rebuild the `objectInfo` list with the new entries from where our `rootObjects` now live.
            objectEntryPoints.Clear();
            for (var i = 0; i < objectData.Count; i++) {
                foreach (var rootObject in rootObjects) {
                    if (objectData[i] == rootObject) {
                        objectEntryPoints.Add((uint) (i + 1));
                    }
                }
            }

            writer.Write(magic);
            writer.Write(version);
            writer.Write(objectEntryPoints.Count);
            writer.Write(instanceInfo.Count);
            writer.Write(userDataInfo.Count);
            writer.Write(reserved);

            var instanceOffsetPos = writer.BaseStream.Position;
            writer.Write(0ul);
            var dataOffsetPos = writer.BaseStream.Position;
            writer.Write(0ul);
            var userDataOffsetPos = writer.BaseStream.Position;
            writer.Write(0ul);

            foreach (var obj in objectEntryPoints) {
                writer.Write(obj);
            }

            writer.WriteValueAtOffset((ulong) writer.BaseStream.Position - rszOffsetStart, instanceOffsetPos);
            foreach (var info in instanceInfo) {
                info.Write(writer);
            }

            writer.PadTill(() => writer.BaseStream.Position % 16 != 0);
            writer.WriteValueAtOffset((ulong) writer.BaseStream.Position - rszOffsetStart, userDataOffsetPos);
            foreach (var userData in userDataInfo) {
                userData.Write(writer);
            }
            foreach (var userData in userDataInfo) {
                userData.UpdateWrite(writer, rszOffsetStart);
            }

            writer.PadTill(() => writer.BaseStream.Position % 16 != 0);
            writer.WriteValueAtOffset((ulong) writer.BaseStream.Position - rszOffsetStart, dataOffsetPos);

            // Call `Write` from the root objects. Those will handle writing out the objects in the correct order.
            foreach (var rootObject in rootObjects) {
                rootObject.Write(writer, testWritePosition);
            }
        }
    }

    public RszObject GetEntryObject() {
        return GetEntryObject<RszObject>();
    }

    public T GetEntryObject<T>() where T : RszObject {
        return (T) objectData[(int) objectEntryPoints[0] - 1];
    }

    public bool TryGetEntryObject<T>(out T? file) where T : RszObject {
        var entryObj = objectData[(int) objectEntryPoints[0] - 1];
        file = entryObj as T;
        return file != null;
    }

    public int AddUserDataRef(uint hash) {
        var nextIndex = userDataInfo.Count;
        userDataInfo.Add(new() {
            instanceId = -1,
            hash       = hash
        });
        return nextIndex;
    }
}
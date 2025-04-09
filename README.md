RE-Editor
---

RE data file editor.

Supported games:

Short | Long | Latest Release | Files Passing Write Tests
--- | --- | --- | ---
DD2 | Dragon Age 2 | [v1.1.7.0](https://github.com/Synthlight/MHR-Editor/releases/tag/DD2-Editor_v1.1.7.0) | 97%
DRDR | Dead Rising Deluxe Remaster | [v1.0.1.0](https://github.com/Synthlight/MHR-Editor/releases/tag/DRDR-Editor_v1.0.1.0) | 86%
MHR | Monster Hunter Rise | [v1.5.1.0](https://github.com/Synthlight/MHR-Editor/releases/tag/MHR-Editor_v1.5.1.0) | 96%
MHWS | Monster Hunter Wilds | [v1.3.1.0](https://github.com/Synthlight/MHR-Editor/releases/tag/MHWS-Editor_v1.3.1.0) | 93%
RE2 | Resident Evil 2 | [v1.0.0.0](https://github.com/Synthlight/MHR-Editor/releases/tag/RE2-Editor_v1.0.0.0) | 79%
RE3 | Resident Evil 3 | [v1.0.0.0](https://github.com/Synthlight/MHR-Editor/releases/tag/RE3-Editor_v1.0.0.0) | 96%
RE4 | Resident Evil 4 | [v1.0.0.0](https://github.com/Synthlight/MHR-Editor/releases/tag/RE4-Editor_v1.0.0.0) | 89%
RE8 | Resident Evil Village | [v1.0.0.0](https://github.com/Synthlight/MHR-Editor/releases/tag/RE8-Editor_v1.0.0.0) | 94%

(Almost everything in the code and prog arguments use the short name; case sensitive.)<br>
To build for a game, switch to that configuration: `{Game} - Debug/Release`

Everything else is on the [wiki](https://github.com/Synthlight/MHR-Editor/wiki). (Though not much.)


Building
---

Game text is pre-extracted and committed to the repo since it's small enough.<br>
Structs/enums however *must* be generated before the editor will compile. See [here](Generator/README.md) for more.
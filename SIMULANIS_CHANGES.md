# Simulanis Unity-MCP Fork - Change Log & Maintenance Notes

This document tracks the specific changes made in the Simulanis Solutions fork of the Unity-MCP project.

**Original Repository:** [https://github.com/IvanMurzak/Unity-MCP](https://github.com/IvanMurzak/Unity-MCP)
**Original Author:** [Ivan Murzak (IvanMurzak)](https://github.com/IvanMurzak)
**Fork Maintainer:** Simulanis Solutions

## Purpose of this Fork

This fork is maintained by Simulanis Solutions to:
1. Integrate the Unity-MCP toolset into Simulanis-specific projects and workflows.
2. Potentially add custom tools or features required by Simulanis.
3. Manage distribution and versioning aligned with Simulanis release cycles.
4. Ensure clear attribution to the original creator while applying necessary branding.

## Distribution Method

This fork is distributed via **GitHub Releases**. Each release will contain a pre-packaged `.tgz` tarball.

**Installation:** Users should download the `.tgz` asset from the desired GitHub Release and install it into their Unity project using a `file:` reference in `Packages/manifest.json`. See the main `README.md` for detailed installation steps.

*(Note: OpenUPM distribution was initially considered but later deferred in favor of direct GitHub Releases.)*

## Key Changes from Original (as of 2025-04-30)

1.  **Rebranding:**
    *   **`Assets/root/package.json`:** Updated `name`, `displayName`, `author`, `description`, `keywords`, and `version` suffix (`-simulanis.N`) to reflect Simulanis branding while retaining attribution.
    *   **`README.md`:** Added fork information, updated titles, badges (pending), and installation commands.
    *   **UI Icon:** Replaced the default icon in the AI Connector window (`Assets/root/Editor/UI/uxml/AiConnectorWindow.uxml`) with the Simulanis logo (`Assets/root/Editor/Gizmos/simulanis-logo.png`). UXML path updated to be relative.

2.  **Package Identification:**
    *   Package name changed to `com.simulanis.unity.mcp`.

3.  **Server Build/Path Logic (`Assets/root/Editor/Scripts/Startup.Server.cs`):**
    *   Modified `ServerSourcePath` getter to use `[CallerFilePath]` to reliably locate the `Server` source directory relative to the installed package location, making local tarball installation viable.
    *   Updated `ServerExecutableRootPath` to use the Simulanis package name (`com.simulanis.unity.mcp`) to create a distinct build directory in the project's `Library` folder (`Library/com.simulanis.unity.mcp/`).
    *   Updated `PackageName` constant to `com.simulanis.unity.mcp`.

4.  **Distribution Strategy:**
    *   Removed OpenUPM configuration files/workflows (`openupm-package.yml`, `.github/workflows/openupm-publish.yml` - *Note: These might need explicit removal if they were committed*).
    *   Focused `TODO.md` and release process around creating `.tgz` packages and using GitHub Releases.

## Synchronization Strategy

This fork aims to stay synchronized with the `main` branch of the upstream `IvanMurzak/Unity-MCP` repository to incorporate bug fixes and new features.

**Frequency:** It is recommended to perform a sync check **monthly**, or more frequently if critical updates are released upstream.

**Process:**

1.  **Ensure Clean State:** Make sure your local `simulanis-dev` branch is up-to-date with `origin/simulanis-dev` and has no uncommitted changes.
2.  **Fetch Upstream:** Run `git fetch upstream` to get the latest changes from the original repository.
3.  **Create Sync Branch:** Create a temporary branch for the sync process: `git checkout -b sync-upstream-YYYYMMDD upstream/main` (replace YYYYMMDD with the date).
4.  **Rebase Development Branch:** Rebase your `simulanis-dev` branch onto this temporary sync branch: 
    *   `git checkout simulanis-dev`
    *   `git rebase sync-upstream-YYYYMMDD` 
    *   *(Alternatively, use `git merge sync-upstream-YYYYMMDD` if you prefer merge commits, but rebase keeps history cleaner.)*
5.  **Resolve Conflicts:** Carefully resolve any merge conflicts that arise during the rebase/merge. 
    *   **Conflict Strategy:** Prioritize keeping the upstream changes unless they directly conflict with a necessary Simulanis modification (like the `Startup.Server.cs` changes). If an upstream change overwrites a Simulanis change, re-apply the Simulanis change *after* accepting the upstream structure.
    *   Document any complex conflict resolutions in the commit message or this file.
6.  **Test After Sync:** Thoroughly test the `simulanis-dev` branch after resolving conflicts (see Testing Checklist below).
7.  **Push Changes:** Once confident, push the updated `simulanis-dev` branch: `git push origin simulanis-dev --force-with-lease` (required if you rebased).
8.  **Clean Up:** Delete the temporary sync branch: `git branch -d sync-upstream-YYYYMMDD`.
9.  **Merge to Main (Optional):** If preparing for a release, merge `simulanis-dev` into `simulanis-main` via a Pull Request.

**Testing Checklist (Post-Sync):**

*   [ ] Project compiles without errors in Unity.
*   [ ] Build MCP Server (`Tools > AI Connector > Build MCP Server`) completes successfully.
*   [ ] AI Connector Window (`Window > AI Connector`) opens and displays correctly (including Simulanis icon).
*   [ ] Connect to AI Client (Cursor/Claude) successfully.
*   [ ] Test core functionalities:
    *   [ ] Get Scene Hierarchy.
    *   [ ] Create/Delete GameObject.
    *   [ ] Modify Component (e.g., Transform).
*   [ ] (If applicable) Test any Simulanis-specific custom tools.
*   [ ] Review major upstream changes noted in their commit history for potential impact.

*(Refer to `TODO.md` Phase 3 for more details)*

## Future Changes

Any future Simulanis-specific modifications (e.g., custom tools) should be documented in this file. 
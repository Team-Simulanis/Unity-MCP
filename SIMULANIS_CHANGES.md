# Simulanis Unity-MCP Fork - Change Log & Maintenance Notes

This document tracks the specific changes made in the Simulanis Solutions fork of the Unity-MCP project.

**Original Repository:** [https://github.com/IvanMurzak/Unity-MCP](https://github.com/IvanMurzak/Unity-MCP)
**Original Author:** [Ivan Murzak (IvanMurzak)](https://github.com/IvanMurzak)
**Fork Maintainer:** Simulanis Solutions

## Version History

### v0.6.2-simulanis.4 (2023-09-03)
- **ui:** Improved Menu_ListItems tool with better formatting and readability
- **fix:** Added table format to display menu items and commands
- **feat:** Updated output to include commands to explore further
- **docs:** Added usage tips section to the output

### v0.6.2-simulanis.3 (2023-09-03)
- **feat:** Simplified menu tools to focus on core functionality
- **fix:** Removed problematic submenu discovery tool (Menu_ListSubmenus)
- **refactor:** Removed documentation tools (Menu_GetReference, Menu_GetMenuReference)
- **cleanup:** Removed test scripts (MenuTest, MenuItemTester, DirectMenuTest)
- **docs:** Updated TODO.md with plans for future improvements to menu tools

### v0.6.2-simulanis.2 (2023-09-01)
- Minor updates and fixes

### v0.6.2-simulanis.1 (2023-04-30)
- Initial branded release of Simulanis fork
- Rebranding and package identification updates
- Modified server build/path logic for better package installation

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

## Commit Message Conventions

We should follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification for commit messages. This helps create a more readable history and can be potentially used for automated changelog generation in the future.

The basic format is:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

Common types include:
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
- `refactor`: A code change that neither fixes a bug nor adds a feature
- `perf`: A code change that improves performance
- `test`: Adding missing tests or correcting existing tests
- `build`: Changes that affect the build system or external dependencies (example scopes: gulp, broccoli, npm)
- `ci`: Changes to our CI configuration files and scripts (example scopes: Travis, Circle, BrowserStack, SauceLabs)
- `chore`: Other changes that don't modify src or test files
- `revert`: Reverts a previous commit

Example:
```
feat(ui): Add Simulanis logo to AI Connector window

Replaced the default icon with the Simulanis brand logo located
in Assets/root/Editor/Gizmos/.
```
```
fix(build): Correct server source path for local package install

Modified Startup.Server.cs to use [CallerFilePath] to ensure the
server source can be found correctly when installing the package
as a local tarball.
```

## Repository Maintenance Guidelines

- **Branching Strategy:**
  - `simulanis-main`: Represents the latest stable release. Merges should only come from `simulanis-dev` via Pull Requests after thorough testing.
  - `simulanis-dev`: The main development branch. All new features, bug fixes, and upstream syncs should be merged or rebased onto this branch first.
  - Feature Branches: Create temporary branches off `simulanis-dev` for significant new features or complex fixes.
- **Upstream Sync:** Follow the process detailed in the "Synchronization Strategy" section above (recommended monthly).
- **Testing:** Always run through the "Testing Checklist (Post-Sync)" after merging/rebasing upstream changes. Test thoroughly before merging `simulanis-dev` into `simulanis-main` for a release.
- **Pull Requests:** Use Pull Requests to merge changes into `simulanis-main` from `simulanis-dev`. Ensure PRs include a clear description of the changes.

- **Issue Tracking:** Use the GitHub Issues tracker for this repository to report bugs or request features specific to the Simulanis fork. 


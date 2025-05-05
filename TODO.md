# Simulanis Unity-MCP Fork - Project Plan

## Phase 1: Initial Setup and Rebranding
- [x] Create fork-specific branches
  - [x] Create `simulanis-main` as our main branch
  - [x] Create `simulanis-dev` for development
  - [ ] Set up branch protection rules

- [x] Rebranding Tasks
  - [x] Update project name references
  - [x] Update package identifiers
  - [x] Modify documentation with Simulanis branding
  - [x] Add Simulanis-specific README sections
  - [x] Update license information with proper attribution
  - [x] Update UI elements (e.g., AI Connector Window Icon)
    - [x] Add new icon file to `Assets/root/Editor/Gizmos`
    - [x] Update `AiConnectorWindow.uxml` to use new icon

- [ ] Package Management for GitHub Release Distribution
  - [x] Update package.json with new identifiers
  - [x] Update relevant manifest files (if applicable)
  - [x] Verify dependencies
  - [ ] Create Distributable Package Structure (Using Git URL now)
    - [x] Build server component (via Unity tool)
    - [x] Adapt Server Source Path Logic (`Startup.Server.cs`)
  - [x] Test Local Package Installation (Testing via Git URL install now)

## Phase 2: Documentation and Attribution
- [x] Update Documentation
  - [x] Add clear attribution to original creator (IvanMurzak)
  - [x] Document fork relationship
  - [ ] Add Simulanis-specific contact information (Placeholder added, needs final info)
  - [x] Update installation instructions (for Git URL method)
    - [x] Explain adding package via Git URL in UPM.
    - [x] Include steps for triggering initial server build.
  - [ ] Add Simulanis-specific use cases
- [ ] Create Contribution Guidelines
  - [ ] Set up PR templates
  - [ ] Define coding standards
  - [ ] Document branch strategy
  - [ ] Establish review process

## Phase 3: Sync Strategy
- [x] Upstream Sync Setup
  - [x] Configure upstream remote
  - [x] Document sync process (in SIMULANIS_CHANGES.md)
  - [x] Create sync schedule (Recommended monthly in docs)
  - [ ] Set up automated sync notifications (Deferred)
- [x] Conflict Resolution Plan
  - [x] Define merge conflict resolution process (In docs)
  - [x] Document testing requirements post-sync (In docs)
  - [x] Create sync validation checklist (In docs)
- [x] Perform Initial Sync with Upstream
  - [x] Fetch upstream changes (`git fetch upstream`)
  - [x] Checkout `simulanis-dev` branch.
  - [x] Rebase `simulanis-dev` onto `upstream/main` (`git rebase upstream/main`).
  - [x] Resolve any merge conflicts.
  - [x] Test thoroughly using checklist in `SIMULANIS_CHANGES.md`.
  - [x] Push updated `simulanis-dev` (`git push origin simulanis-dev --force-with-lease`).

## Phase 4: Release Management (GitHub)
- [x] Initial Release Strategy (Switched to Git URL)
  - [x] Define version numbering strategy (e.g., `vX.Y.Z-simulanis.N`)
  - [x] Create release checklist
    - [x] Ensure `simulanis-main` branch is up-to-date.
    - [x] Test install using Git URL with the release tag.
  - [x] Draft release notes template (including installation steps).
- [x] GitHub Release Workflow Automation (Using Git URL)
  - [x] Create GitHub Actions workflow (`.github/workflows/github-release.yml`).
  - [x] Configure workflow to trigger on tag push (e.g., `v*`).
  - [x] Add steps to checkout code.
  - [x] Add step to automatically create GitHub Release for the tag (without assets).
  - [x] Test the workflow with the `v0.6.2-simulanis.1` tag (Successful).
  - [x] Resolved issue with conflicting `openupm-publish.yml` workflow triggering.
    - [x] Deleted `openupm-publish.yml`.
    - [x] Deleted and re-created/pushed tag `v0.6.2-simulanis.1` to point to correct commit.
- [x] Manual Release Steps (Confirmed Working)
  - [x] Create Git tag for release (e.g., `git tag v0.6.2-simulanis.1`).
  - [x] Push the tag to remote (`git push origin v0.6.2-simulanis.1`).
  - [x] Go to GitHub repository > Releases > "Draft a new release".
  - [x] Choose the pushed tag.
  - [x] Set release title (e.g., `v0.6.2-simulanis.1`).
  - [x] Write release notes (using template, link to installation guide).
  - [x] Publish release.
- [ ] Maintenance Plan
  - [ ] Define update frequency
  - [ ] Create hotfix process
  - [ ] Document version control workflow

## Phase 5: Future Improvements (Post-Initial Release)
- [ ] Custom Tools Development
  - [x] Identify Simulanis-specific tool needs
    - [x] Remote Menu Item Execution tool (planned and added to task list)
    - [ ] Scene management utilities (create/load/save scenes remotely)
    - [ ] Asset creation and manipulation tools
    - [ ] Build automation and distribution commands
    - [ ] Project settings management
  - [ ] Plan tool architecture
    - [ ] Design a modular command system for easy addition of new tools
    - [ ] Create standard response format for all commands
    - [ ] Implement client-side validation for commands
    - [ ] Add permission/security model for sensitive operations
  - [ ] Create development roadmap
    - [ ] Prioritize tools based on team workflow impact
    - [ ] Schedule implementation phases
    - [ ] Define KPIs to measure tool effectiveness

- [ ] Integration Requirements
  - [ ] List Simulanis-specific integrations
    - [ ] Integration with existing Simulanis automation scripts
    - [ ] Content pipeline workflow integration
    - [ ] CI/CD systems integration
    - [ ] Analytics integration for usage tracking
  - [ ] Document integration points
    - [ ] Define API contract for external systems
    - [ ] Create integration examples/templates
    - [ ] Implement authentication for external systems
  - [ ] Plan testing strategy
    - [ ] Unit tests for command handlers
    - [ ] Integration tests for Unity <-> Server communication
    - [ ] End-to-end tests with actual Unity projects
    - [ ] Performance/stress testing

- [ ] User Experience Improvements
  - [ ] Create an improved UI for remote connection management
  - [ ] Add progress reporting for long-running remote operations
  - [ ] Implement better error reporting and recovery options
  - [ ] Add connection health monitoring and auto-reconnect features

- [ ] Documentation and Training
  - [ ] Create comprehensive API documentation
  - [ ] Develop example projects demonstrating key features
  - [ ] Create video tutorials for common workflows
  - [ ] Write best practices guide for remote Unity automation

## Notes
- Original Project: [Unity-MCP](https://github.com/IvanMurzak/Unity-MCP)
- Original Creator: [Ivan Murzak](https://github.com/IvanMurzak)
- Fork Maintainer: Simulanis Solutions

## Important Reminders
1. Always maintain proper attribution to the original project
2. Keep track of upstream changes
3. Document all Simulanis-specific modifications
4. Maintain test coverage for custom additions
5. Regular sync with upstream to stay updated

## Dependencies
- Unity Version Support: 2022.3.61f1 and newer
- .NET 9.0

## Regular Maintenance Tasks
- [ ] Weekly
  - [ ] Check for upstream updates
  - [ ] Review open issues
  - [ ] Test integration points

- [ ] Monthly
  - [ ] Full sync with upstream
  - [ ] Documentation review
  - [ ] Version compatibility check

- [ ] Quarterly
  - [ ] Major version planning
  - [ ] Feature roadmap review
  - [ ] Dependencies audit

# TODO List for Simulanis Unity-MCP Fork

[x] Feature: Remote Menu Item Execution - Define command payloads (GetExposedMenuItems, ExecuteMenuItemCommand) in Common project.
[x] Feature: Remote Menu Item Execution - Implement Unity client handler for GetExposedMenuItems (define initial list, send back).
[x] Feature: Remote Menu Item Execution - Implement Unity client handler for ExecuteMenuItemCommand (use EditorApplication.ExecuteMenuItem on main thread, add error handling).
[x] Feature: Remote Menu Item Execution - Implement Server Hub methods to trigger client handlers and relay results.
[x] Feature: Remote Menu Item Execution - Create basic test mechanism/client to list and execute items.
[x] Feature: Remote Menu Item Execution - Perform end-to-end testing with initial predefined items (e.g., Save Project, Refresh).
[x] Feature: Remote Menu Item Execution - Integrate user's custom script menu items (identify paths, add to exposed list).
[x] Feature: Remote Menu Item Execution - Test execution of user's custom script menu items.
[x] Feature: Remote Menu Item Execution - Document feature in README/SIMULANIS_CHANGES. 

# Current Status (Menu Tools Implementation)

## Completed
- [x] Implemented Menu_ListItems tool to retrieve menu items from Unity
- [x] Implemented Menu_ExecuteItem tool to execute menu items in Unity
- [x] Created both Editor-side and Server-side implementations for the menu tools
- [x] Successfully tested the tools in the Unity-MCP environment
- [x] Tools are showing up in the MCP tool list (total 43 tools)

## Known Issues & Future Improvements
- [ ] Improve menu item categorization and hierarchy navigation
- [ ] Add better filtering options for menu items
- [ ] Add caching mechanism to improve performance
- [ ] Implement better error handling and reporting
- [ ] Create visual UI for browsing and executing menu items
- [ ] Add permission system to restrict access to certain menu items

## Next Steps
- [ ] Further testing with more complex menu structures
- [ ] Performance optimization for large menu hierarchies
- [ ] Documentation updates to include examples of using the menu tools
- [ ] Integration with other Unity-MCP tools for enhanced workflows

## Menu Tools Implementation Status

### Completed Features:
- ✅ Menu_ListItems - Lists available menu items in a specified category/path
- ✅ Menu_ExecuteItem - Executes a menu item by its full path

### Removed Features:
- ❌ Menu_ListSubmenus - Removed due to implementation issues 
- ❌ Menu_GetReference - Removed to simplify the tool set
- ❌ Menu_GetMenuReference - Removed to simplify the tool set
- ❌ Test Scripts - Removed test utilities for menu functionality:
  - MenuTest.cs
  - MenuItemTester.cs
  - DirectMenuTest.cs

### TODO for Submenu Functionality:
- [ ] Redesign the submenu discovery approach
- [ ] Implement a more robust algorithm for finding nested menus
- [ ] Add proper caching to improve performance with large menu hierarchies
- [ ] Create comprehensive test cases for various menu structures
- [ ] Reintroduce the submenu functionality after thorough testing

### TODO for Documentation Tools:
- [ ] Evaluate if documentation tools are needed in future releases
- [ ] Consider integrating documentation into Menu_ListItems directly
- [ ] Explore more efficient ways to represent menu hierarchy

### TODO for Testing Tools:
- [ ] Reimplement menu testing utilities with more robust functionality
- [ ] Create a unified test framework for menu operations
- [ ] Add automated tests for menu discovery and execution
- [ ] Develop visual test tools to help debug menu hierarchy issues
- [ ] Create developer documentation on how to use test utilities

### Next Steps:
1. Test the simplified toolset after server restart in Unity
2. For any custom menu tools/items added in the future, use the following approach:
   - Create menu items with a consistent naming structure (e.g., "Tools/McpTool/MyFeature")
   - Document them with comments that describe their purpose

### How to Use the Menu System:
1. List top-level menus: `Menu_ListItems("")`
2. List items in a category: `Menu_ListItems("File")`  
3. List items in a nested menu: `Menu_ListItems("Tools/AI Connector (Unity-MCP)")`
4. Execute a menu item: `Menu_ExecuteItem("File/New Scene %n")`
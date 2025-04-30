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
- [ ] Perform Initial Sync with Upstream
  - [ ] Fetch upstream changes (`git fetch upstream`)
  - [ ] Checkout `simulanis-dev` branch.
  - [ ] Rebase `simulanis-dev` onto `upstream/main` (`git rebase upstream/main`).
  - [ ] Resolve any merge conflicts.
  - [ ] Test thoroughly using checklist in `SIMULANIS_CHANGES.md`.
  - [ ] Push updated `simulanis-dev` (`git push origin simulanis-dev --force-with-lease`).

## Phase 4: Release Management (GitHub)
- [ ] Initial Release Strategy
  - [ ] Define version numbering strategy (e.g., `vX.Y.Z-simulanis.N`)
  - [ ] Create release checklist
    - [ ] Ensure `simulanis-main` branch is up-to-date.
    - [ ] Test install using Git URL with the release tag.
  - [ ] Draft release notes template (including installation steps).
- [ ] GitHub Release Workflow Automation [Needs Update for Git URL]
  - [x] Create GitHub Actions workflow (`.github/workflows/github-release.yml`).
  - [x] Configure workflow to trigger on tag push (e.g., `v*`).
  - [x] Add steps to checkout code.
  - [x] Add step to automatically create GitHub Release for the tag (without assets).
  - [ ] Test the workflow with a pre-release tag.
- [ ] Manual Release Steps (if automation fails or for first release)
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
  - [ ] Identify Simulanis-specific tool needs
  - [ ] Plan tool architecture
  - [ ] Create development roadmap

- [ ] Integration Requirements
  - [ ] List Simulanis-specific integrations
  - [ ] Document integration points
  - [ ] Plan testing strategy

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
Here are the 6 Techne skills installed at \~/.techne/skills/:

|Skill|ID|Tags|
|-|-|-|
|Git Commit Message|git-commit-message|git, commit, conventional-commits|
|Git Release Manager|git-release-manager|git, release, semver, github|
|Git Release Cherry Pick|git-release-cherry-pick|git, release, cherry-pick, dev|
|Git Release Promote|git-release-promote|git, release, promotion, quality-gate|
|Git Sync Dev|git-sync-dev|git, dev, sync, release|
|GH Issue Create|gh-issue-create|github, gh, issue|

How to invoke them — per the project instructions, before executing any git release operation, I read the relevant skill file first. 

You can trigger them by asking me to perform the related task:

- "Write a commit message" → loads git-commit-message.md
- "Create a release branch" / "Cut a release" → loads git-release-manager.md
- "Cherry-pick commits for release" → loads git-release-cherry-pick.md
- "Promote release to stage/prod" / "Run promotion gates" → loads git-release-promote.md
- "Sync dev with release" → loads git-sync-dev.md
- "Create a GitHub issue" → loads gh-issue-create.md


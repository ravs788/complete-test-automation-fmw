# Repository instructions

## Personal defaults

- Use NEMS MCP for Jira, Confluence, and other services available through NEMS MCP.
- Use Playwright CLI in headed mode for test execution, unless the user has explicitly authorized an exception covering the work.
- Close tabs opened for a Playwright CLI run as soon as they are no longer needed, and close browser windows opened for testing when the run is complete. Preserve unrelated user tabs and windows.

## Required reviews before every commit

**Commit-request trigger:** Whenever the user asks to commit (including "commit", "commit these changes", "commit and push", or an amendment), automatically run all five review skills below against the changes intended for that commit before invoking Git to create it. Do not require a separate request to review. A previous repository review does not replace this commit-specific review; reuse matching verification evidence only as described below. This rule applies on every branch and to future sessions that read this file.

Before creating any commit in this repository, on any branch, apply all five skills:

1. `$code-review`
2. `$test-review`
3. `$coverage-review`
4. `$documentation-review`
5. `$test-modernization-skill`

This requirement includes documentation-only commits, follow-up commits, and amendments. Read each skill's instructions and perform its workflow; merely listing a skill or reading its SKILL.md does not count as completing the review.

Review the exact staged changes plus the surrounding implementation/configuration needed to understand their impact. Scope the reviews to the change rather than automatically repeating a whole-solution audit. If a skill finds no applicable changes, report that conclusion with a brief reason after inspecting the staged diff. The modernization assessment must include the current online research required by that skill, proportionate to the change.

Complete the reviews before committing. If subsequent edits change the staged content, revisit the affected review conclusions before the commit. Existing verification results may be reused only when the code, environment and assertions they support still match; label reused or historical evidence accurately.

Address findings within the authorized task scope and summarize any remaining findings, deferred recommendations, skipped checks and execution limits. Running these reviews does not automatically authorize implementing every suggestion, adding dependencies, changing deferred mobile work, or provisioning external services. Optional modernization suggestions do not by themselves block an otherwise authorized commit.

If a required skill is missing or a mandatory review step cannot be completed, do not silently skip it or claim the gate passed. State the concrete blocker and this rule as its source before committing; proceed only when the requirement can be met or the user explicitly grants an exception. Honor exceptions already authorized for the applicable work without requesting them again.

In the commit handoff, give a concise outcome for each of the five reviews and identify any residual findings. A separate tracked report is required only when requested or required by an existing repository workflow. Do not claim tests, measured coverage, current online verification, or independent review that did not occur.

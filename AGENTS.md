# Repository instructions

## Personal defaults

- Use NEMS MCP for Jira, Confluence, and other services available through NEMS MCP.
- Use Playwright CLI in headed mode for test execution, unless the user has explicitly authorized an exception covering the work.
- Close tabs opened for a Playwright CLI run as soon as they are no longer needed, and close browser windows opened for testing when the run is complete. Preserve unrelated user tabs and windows.

## Required reviews before every commit

**Commit-request trigger:** Whenever the user asks to commit (including "commit", "commit these changes", "commit and push", or an amendment), automatically run all six review skills below against the changes intended for that commit before invoking Git to create it. Do not require a separate request to review. A previous repository review does not replace this commit-specific review; reuse matching verification evidence only as described below. This rule applies on every branch and to future sessions that read this file.

Before creating any commit in this repository, on any branch, apply all six skills:

1. `$code-review`
2. `$test-review`
3. `$coverage-review`
4. `$documentation-review`
5. `$test-modernization-skill`
6. `$security-review`

This requirement includes documentation-only commits, follow-up commits, and amendments. Read each skill's instructions and perform its workflow; merely listing a skill or reading its SKILL.md does not count as completing the review.

Review the exact staged changes plus the surrounding implementation/configuration needed to understand their impact. Scope the reviews to the change rather than automatically repeating a whole-solution audit. If a skill finds no applicable changes, report that conclusion with a brief reason after inspecting the staged diff. The modernization assessment must include the current online research required by that skill, proportionate to the change. The security review must assess applicable controls across the changed code/configuration and relevant trust boundaries, verify standards/advisory claims against current primary sources, and record unverified or inapplicable controls explicitly. Keep security checks in code-review and deduplicate overlapping findings with security-review.

Complete the reviews before committing. If subsequent edits change the staged content, revisit the affected review conclusions before the commit. Existing verification results may be reused only when the code, environment and assertions they support still match; label reused or historical evidence accurately.

Address findings within the authorized task scope and summarize any remaining findings, deferred recommendations, skipped checks and execution limits. Running these reviews does not automatically authorize implementing every suggestion, adding dependencies, changing deferred mobile work, or provisioning external services. Optional modernization suggestions do not by themselves block an otherwise authorized commit.

If a required skill is missing or a mandatory review step cannot be completed, do not silently skip it or claim the gate passed. State the concrete blocker and this rule as its source before committing; proceed only when the requirement can be met or the user explicitly grants an exception. Honor exceptions already authorized for the applicable work without requesting them again.

In the commit handoff, give a concise outcome for each of the six reviews and identify any residual findings. Record the reviews in docs/REVIEW.md as required below; an additional report is optional unless requested. Do not claim tests, measured coverage, current online verification, or independent review that did not occur.

## Required review records

Whenever any of the six review skills above is used in this repository, record its substantive findings and actionable recommendations in `docs/REVIEW.md` during the same task. This is standing authorization to maintain that ledger; no separate request to save findings is needed. It applies to standalone reviews as well as pre-commit reviews, including findings fixed during the same task.

- Reuse the existing finding ID when the issue is already tracked. Deduplicate overlapping inputs from different skills and attribute all relevant skills; allocate the next available R-number only for a new item. Preserve closed history and explicit deferrals unless new evidence or the user's instruction changes them.
- Include the date, originating skill(s), reviewed revision/working-tree scope, priority, affected file/line evidence, concrete consequence or rationale, proposed action, status and verification/acceptance criteria. Distinguish confirmed defects, verification gaps and optional design/modernization/security-hardening recommendations. Do not present an unverified concern as a confirmed vulnerability or an optional suggestion as a required fix.
- Keep the findings table, details, anchors and current counts consistent. Record actual implementation/verification evidence when closing an item; never invent results or commit references. Redact sensitive security evidence and never write credentials or tokens into the ledger.
- Add a concise dated review summary covering scope, the skill outcomes and verification limits. If a skill finds no issues or has no applicable changes, record that outcome with its reason in the summary rather than inventing an R-number.
- For a commit request, include the resulting ledger updates in the reviewed staged changes before committing. If review work changes the staged content, revisit the affected conclusions. Updating the ledger does not authorize implementing every recommendation or resuming deferred work.

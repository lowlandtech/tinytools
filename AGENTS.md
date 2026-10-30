# Tinytools agent execution

These lane rules supplement `D:/lowlandtech/AGENTS.md`. Follow the shared
`agentic-scrum`, `agentic-gherkin` and `eng-tools` skills along with the
Tinytools workitem board and this repository's issue and delivery rules.

## Keep working after completing an item

Finishing a workitem is a checkpoint inside the current run, not a reason to
stop. Complete the source, metadata, board and managed issue transition using
the Tinytools workflow, record exact verification and delivery evidence, then
immediately recheck the Tinytools inbox, shared queue and source board. Claim
the next eligible, released TODO item and continue its implementation in the
same run. Do not wait for coordinator review, issue-mirror confirmation or the
next scheduled trigger before starting independent ready work.

Continue sequentially until no eligible released work remains, a concrete gap
blocks every remaining eligible item, the user stops the run, or a hard runtime
limit requires a truthful checkpoint. A blocked item does not stop the lane:
record its exact prerequisite, evidence, owner and recovery action, then keep
working an independent eligible item. Do not invent work or change scope to
avoid reporting a genuine blocker. Preserve repository leases and any rule that
requires one active item at a time; when that rule applies, pick the next item
immediately after closing the current one.

Use the normal source-file, metadata and `docs/workitems/README.md` transaction
for every state change. A chat message or a closed GitHub issue does not replace
that transaction or prove acceptance. For behavior-bearing tests, use the
shared Agentic Gherkin grammar from the first edit and run the scoped diagnostic
and behavioral checks required by the source workitem before completion.

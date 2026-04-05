# CA2 Branch Strategy

For CA1 I committed directly to main with no branches. The entire CA1 was done in a single session on March 2nd and the commit history reflects that. For CA2 I created `feature/ca2-network` from main to keep all Fusion networking work separate from the stable project. All networking scripts, prefabs, scene changes, and documentation went on this branch. Once the pickup feature was tested and stable I merged it back to main and applied the `ca2-submit` tag.

## Tags
`baseline` on the initial project setup, `ca1-submit` on the CA1 submission, `ca2-baseline` on the commit where two-client sessions first connected, and `ca2-submit` on the final submission state.

## What I'd Do Differently
I would start using feature branches from the beginning of the module rather than only introducing them for CA2. I would also spread work across the teaching weeks instead of compressing everything into single sessions. The `feature/ca2-network` branch itself worked well for isolating the networking sprint from the CA1 scene and the merge back to main was clean. The problem is everything before it had no structure at all.
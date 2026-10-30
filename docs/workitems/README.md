| Milestone | Title | Gate |
|---|---|---|
| M1 | Correctness and test foundations | Open correctness regressions, visible use-case test specifications and RazorLight cache compatibility have executable coverage, accepted source and matching issue states. |
| M2 | Developer experience | Validation, caching, syntax diagnostics, additional helpers and the published performance target have verified acceptance. |
| M3 | Advanced templating | Includes, macros, asynchronous services and collection helpers meet their contracts. |
| M4 | Ecosystem and tooling | Editor, source generation, reusable template packs and CLI delivery are accepted. |

| Work | Scope | Title | Priority | Lane | Depends on | Status | Refined | Issue |
|---|---|---|---|---|---|---|---|---|
| M1-001 | SPEC3300-US03-SC02 | [Preserve comparison operator precedence](todo/SPEC3300.US03.SC02.operator-precedence.md) | P1 | tinytools | None | todo | true | [#10](https://github.com/lowlandtech/tinytools/issues/10) |
| M1-002 | SPEC3300-US03-SC03 | [Compare floating-point values with a tolerance](todo/SPEC3300.US03.SC03.floating-point-equality.md) | P3 | tinytools | None | todo | true | [#12](https://github.com/lowlandtech/tinytools/issues/12) |
| M1-003 | SPEC3305-US02-SC01 | [Convert existing tests to visible use-case specifications](todo/SPEC3305.US02.SC01.visible-use-case-test-specifications.md) | P0 | tinytools | None | todo | true | [#52](https://github.com/lowlandtech/tinytools/issues/52) |
| M1-004 | SPEC3300-US11-SC01 | [Restore and fully test RazorLight cache compatibility](todo/SPEC3300.US11.SC01.razorlight-cache-compatibility.md) | P1 | tinytools | None | todo | true | [#53](https://github.com/lowlandtech/tinytools/issues/53) |
| M2-001 | SPEC3302-US04-SC01 | [Validate template syntax before rendering](todo/SPEC3302.US04.SC01.pre-render-syntax-validation.md) | P1 | tinytools | None | todo | true | [#44](https://github.com/lowlandtech/tinytools/issues/44) |
| M2-002 | SPEC3300-US07-SC01 | [Compile and reuse templates safely](todo/SPEC3300.US07.SC01.compiled-template-cache.md) | P1 | tinytools | None | todo | false | [#39](https://github.com/lowlandtech/tinytools/issues/39) |
| M2-003 | SPEC3300-US08-SC01 | [Report template syntax locations](todo/SPEC3300.US08.SC01.syntax-diagnostic-locations.md) | P1 | tinytools | M2-001 | todo | true | [#40](https://github.com/lowlandtech/tinytools/issues/40) |
| M2-004 | SPEC3303-US03-SC02 | [Add slice, match, contains and split helpers](todo/SPEC3303.US03.SC02.additional-pipe-helpers.md) | P2 | tinytools | None | todo | true | [#45](https://github.com/lowlandtech/tinytools/issues/45) |
| M2-005 | SPEC3300-US06-SC02 | [Measure and improve variable-resolution performance](todo/SPEC3300.US06.SC02.variable-resolution-performance.md) | P2 | tinytools | None | todo | false | [#38](https://github.com/lowlandtech/tinytools/issues/38) |
| M3-001 | SPEC3300-US09-SC01 | [Compose templates from includes and partials](todo/SPEC3300.US09.SC01.template-includes.md) | P2 | tinytools | None | todo | false | [#41](https://github.com/lowlandtech/tinytools/issues/41) |
| M3-002 | SPEC3300-US10-SC01 | [Define and invoke template macros](todo/SPEC3300.US10.SC01.template-macros.md) | P2 | tinytools | None | todo | false | [#42](https://github.com/lowlandtech/tinytools/issues/42) |
| M3-003 | SPEC3303-US04-SC01 | [Support asynchronous template services](todo/SPEC3303.US04.SC01.async-template-services.md) | P2 | tinytools | None | todo | false | [#47](https://github.com/lowlandtech/tinytools/issues/47) |
| M3-004 | SPEC3303-US03-SC03 | [Add collection filtering, ordering, grouping and distinct helpers](todo/SPEC3303.US03.SC03.collection-helpers.md) | P2 | tinytools | None | todo | false | [#46](https://github.com/lowlandtech/tinytools/issues/46) |
| M4-001 | SPEC3306-US01-SC01 | [Build a Visual Studio template editing extension](todo/SPEC3306.US01.SC01.visual-studio-extension.md) | P2 | tinytools | None | todo | false | [#48](https://github.com/lowlandtech/tinytools/issues/48) |
| M4-002 | SPEC3306-US02-SC01 | [Generate typed template renderers at build time](todo/SPEC3306.US02.SC01.source-generator-templates.md) | P2 | tinytools | None | todo | false | [#49](https://github.com/lowlandtech/tinytools/issues/49) |
| M4-003 | SPEC3301-US02-SC01 | [Publish and discover reusable template packs](todo/SPEC3301.US02.SC01.template-library-registry.md) | P2 | tinytools | None | todo | false | [#43](https://github.com/lowlandtech/tinytools/issues/43) |
| M4-004 | SPEC3306-US03-SC01 | [Complete the template render and watch CLI](todo/SPEC3306.US03.SC01.render-watch-cli.md) | P2 | tinytools | None | todo | false | [#50](https://github.com/lowlandtech/tinytools/issues/50) |

# Code Review Report

**File(s) Reviewed:** {{files}}
**Reviewer:** AI Assistant
**Date:** {{date}}
**Review Type:** {{review_type}}

---

## Summary

{{summary}}

**Overall Assessment:** {{assessment}}

---

## Findings

### Critical Issues

{{#critical_findings}}
- **[{{category}}]** {{description}}
  - Location: `{{file}}:{{line}}`
  - Impact: {{impact}}
  - Recommendation: {{recommendation}}
{{/critical_findings}}

{{^critical_findings}}
No critical issues found.
{{/critical_findings}}

### Warnings

{{#warning_findings}}
- **[{{category}}]** {{description}}
  - Location: `{{file}}:{{line}}`
  - Recommendation: {{recommendation}}
{{/warning_findings}}

{{^warning_findings}}
No warnings found.
{{/warning_findings}}

### Suggestions

{{#suggestion_findings}}
- **[{{category}}]** {{description}}
  - Location: `{{file}}:{{line}}`
  - Suggestion: {{suggestion}}
{{/suggestion_findings}}

{{^suggestion_findings}}
No suggestions at this time.
{{/suggestion_findings}}

---

## Statistics

| Metric | Value |
|--------|-------|
| Files Reviewed | {{file_count}} |
| Lines Analyzed | {{line_count}} |
| Critical Issues | {{critical_count}} |
| Warnings | {{warning_count}} |
| Suggestions | {{suggestion_count}} |

---

## Recommendation

{{recommendation}}

**Merge Status:** {{merge_status}}

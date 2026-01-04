# Team Code Review Guidelines

## Purpose

These guidelines ensure consistent, constructive code reviews that improve code quality while maintaining team velocity.

## Review Philosophy

1. **Be Kind**: Reviews are about code, not people
2. **Be Constructive**: Suggest improvements, don't just criticize
3. **Be Timely**: Review within 24 hours when possible
4. **Be Thorough**: But also pragmatic about scope

---

## Must Block (Critical)

These issues MUST be resolved before merging:

### Security
- Any security vulnerability (OWASP Top 10)
- Hardcoded credentials or secrets
- Missing authentication/authorization checks
- SQL injection, XSS, or command injection risks

### Data Integrity
- Risk of data loss or corruption
- Missing database transactions for multi-step operations
- Race conditions in concurrent code

### Breaking Changes
- Unversioned breaking API changes
- Missing migrations for schema changes
- Removed public interfaces without deprecation period

---

## Should Fix (Warning)

These issues should be fixed but won't block merge if there's a good reason:

### Code Quality
- Methods over 50 lines
- Cyclomatic complexity > 10
- Obvious code duplication (3+ instances)
- Missing error handling in public APIs

### Testing
- No tests for new public APIs
- Removed tests without justification
- Flaky or non-deterministic tests

### Documentation
- Missing XML documentation on public members
- Outdated or incorrect comments
- Missing README updates for new features

---

## Nice to Have (Suggestion)

These are improvements that would be nice but are not required:

- Variable naming improvements
- Minor performance optimizations
- Additional test coverage for edge cases
- Code style preferences (when not enforced by linter)
- Refactoring opportunities

---

## Review Checklist

### Before Reviewing
- [ ] Understand the context (read PR description, linked issues)
- [ ] Pull and run locally if changes are significant
- [ ] Check CI status

### During Review
- [ ] Security implications considered
- [ ] Error handling adequate
- [ ] Tests cover new/changed behavior
- [ ] Documentation updated if needed
- [ ] No obvious performance issues

### After Review
- [ ] Clear, actionable feedback provided
- [ ] Severity levels assigned appropriately
- [ ] Positive aspects acknowledged

---

## Response Time Expectations

| PR Size | Expected Review Time |
|---------|---------------------|
| XS (<50 lines) | Same day |
| S (50-200 lines) | 1 business day |
| M (200-500 lines) | 2 business days |
| L (500+ lines) | Consider splitting |

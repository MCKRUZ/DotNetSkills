---
name: Code Review Assistant
description: Performs structured code reviews. Use when reviewing PRs, commits, or code files for quality, security, and maintainability issues.
version: "1.0.0"
author: Skills Quickstart
category: development
tags:
  - development
  - quality
  - security
  - code-review
---
# Code Review Assistant

## Overview

This skill provides structured code review capabilities following industry best practices. It helps identify security vulnerabilities, code quality issues, performance problems, and maintainability concerns.

## When to Use This Skill

Invoke this skill when:
- Reviewing pull requests or merge requests
- Auditing code for security vulnerabilities
- Assessing code quality before release
- Onboarding to understand existing code patterns

## Instructions

When reviewing code, analyze in this order:

### 1. Security Analysis

- Check for injection vulnerabilities (SQL, XSS, command injection)
- Look for hardcoded secrets or credentials
- Verify input validation and sanitization
- Check authentication/authorization logic
- Review cryptographic implementations

### 2. Code Quality

- Assess naming conventions and readability
- Check cyclomatic complexity
- Look for code duplication (DRY violations)
- Verify error handling patterns
- Check for proper resource disposal

### 3. Performance

- Identify N+1 query patterns
- Check for unnecessary allocations
- Look for blocking calls in async code
- Verify proper use of caching
- Check for memory leaks

### 4. Maintainability

- Check for SOLID principle violations
- Assess test coverage needs
- Look for missing documentation
- Verify consistent patterns
- Check dependency management

## Output Format

Use the template at `templates/review-report.template.md` to structure your findings. This ensures consistent, actionable feedback.

## Team Standards

Consult `references/review-guidelines.md` for team-specific standards and what constitutes blocking vs. non-blocking issues.

## Severity Levels

- **Critical**: Security vulnerabilities, data loss risks - must block merge
- **Warning**: Code quality issues, potential bugs - should fix before merge
- **Suggestion**: Style improvements, minor optimizations - nice to have

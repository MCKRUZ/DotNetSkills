---
name: EY Executive Deck Generator
description: Creates executive-level PowerPoint presentations using EY brand guidelines, SCR narrative framework, and assertive headline methodology. Use when creating client-facing decks for strategic recommendations.
version: 1.0.0
author: EY Transformation
category: presentation-generation
tags:
  - presentation
  - powerpoint
  - executive-communication
  - scr-framework
  - ey-brand
  - consulting
resources:
  templates: 2
  references: 2
  scripts: 0
  assets: 2
---

# EY Executive Deck Generator

## Overview

This skill generates professional executive presentations following EY's proven methodology for strategic communication. It creates decks that are visually consistent with EY brand guidelines and structured using the Situation-Complication-Resolution (SCR) narrative framework.

## When to Use This Skill

Invoke this skill when:
- Creating client-facing executive presentations
- Developing strategic recommendation decks
- Presenting transformation roadmaps
- Communicating complex business initiatives to leadership
- Generating pitch decks for new opportunities

## Prerequisites

- Clear understanding of the recommendation being made
- Target audience identification (C-suite, VPs, Board)
- Key data points and metrics to support the narrative
- Decision timeline and stakes context

## Instructions

### Step 1: Assess Presentation Requirements

Before generating content, determine:

1. **Decision Context**: Is there a decision to be made? How high-stakes is it?
2. **Audience Resistance**: Do you anticipate resistance from the audience?
3. **Complexity**: How complex is the subject matter?
4. **Data Availability**: What supporting metrics and evidence exist?

### Step 2: Select Narrative Framework

Use the `RecommendFramework` tool to determine the best approach:

- **SCR (Situation-Complication-Resolution)**: Default for decisions, high-stakes, or anticipated resistance
- **Past-Present-Future**: For transformation journeys or progress updates
- **Problem-Solution**: For straightforward challenges with clear solutions
- **Opportunity-Approach**: For positive, opportunity-focused narratives

### Step 3: Gather Brand Guidelines

Always load brand guidelines from `assets/brand/ey-brand.md` before creating visual elements. This ensures:
- Proper color palette application (EY Yellow, Navy, Black)
- Typography standards (EY Sans, EY Serif)
- Logo usage and placement rules
- Slide layout principles

### Step 4: Generate Assertive Headlines

Every slide title should be an **assertive headline**, not a label. Use the `GenerateAssertiveHeadline` tool to transform topic labels into compelling headlines:

- **Weak label**: "Market Overview"
- **Assertive headline**: "Market dynamics present a $50M opportunity through digital transformation"

- **Weak label**: "Current State Analysis"
- **Assertive headline**: "Legacy operations limit growth and increase operational risk by 40%"

### Step 5: Structure Slide Content

For each slide, use the `GenerateSlideStructure` tool to ensure:
1. **Single message per slide** - one key insight
2. **Evidence-based assertions** - data points support the headline
3. **Visual hierarchy** - headline dominates, body text supports
4. **Action orientation** - what should the audience do/think/feel?

### Step 6: Generate PowerPoint

Use the `CreatePresentation` tool with:
- **Title and Subtitle**: Compelling, assertive, specific
- **Slides JSON**: Structured array of slide objects with headlines, content, and layout types
- **Template**: Specify `ey-dark` or `ey-light` based on audience preference

## Slide Types

### Title Slide
- Assertive headline as title
- Subtitle with context/stakes
- Presenter info and date

### Agenda Slide
- 3-5 key points
- Each point framed as a takeaway, not a topic

### Content Slides
Choose appropriate layout:
- **Headline-Body-Body-Body**: Three supporting points
- **Headline-Graphic-Support**: Visual evidence with commentary
- **Headline-Data-Insight**: Chart with key takeaway
- **Headline-Quote-Attribution**: testimonials or external validation

### Action Slide
- Clear call to action
- Timeline and next steps
- Ownership and accountability

## Best Practices

1. **Start with the story**: Outline the SCR narrative before designing slides
2. **Write headlines first**: Every slide must work as a standalone unit
3. **Limit text**: Use bullet points, not paragraphs
4. **Use visuals strategically**: Charts, diagrams, photos should advance the story
5. **Brand consistency**: Follow EY guidelines for all visual elements
6. **Rehearse the narrative**: The deck should tell a coherent story

## Tools Available

- **LoadResource**: Load reference materials or brand guidelines
- **RecommendFramework**: Get recommendation for narrative framework
- **GenerateAssertiveHeadline**: Transform topic labels into assertive headlines
- **GenerateSlideStructure**: Get slide outline for a specific framework
- **CreatePresentation**: Generate final PowerPoint .pptx file

## Output

Generates a PowerPoint file (.pptx) with:
- EY brand-compliant design
- Assertive headlines on every slide
- SCR narrative structure
- Professional visual hierarchy
- Ready for executive presentation

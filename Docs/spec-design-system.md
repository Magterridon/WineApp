# Wine Cellar App — Design System and UI Specification

## Goal

Define the shared UI and UX direction for the app.

The app should feel:
- modern
- elegant
- refined
- warm
- premium
- consumer-facing
- wine/lifestyle-oriented

It must not feel like:
- a Bootstrap app
- a generic CRUD interface
- a default admin dashboard
- a developer-first internal tool

---

## Global design principles

- no Bootstrap in the frontend
- use custom shared styling and reusable UI patterns
- prefer restraint over visual noise
- prioritize typography, spacing, hierarchy, and polish
- create consistency across pages
- keep accessibility and readability strong
- keep responsive behavior solid

---

## Visual direction

### Tone
- premium but simple
- editorial/lifestyle inspired
- elegant without being flashy
- modern without looking cold

### Palette
- restrained and cohesive
- avoid harsh default blue product styling unless clearly justified
- use color intentionally, not everywhere
- preserve accessible contrast

### Typography
- clear visual hierarchy
- strong page titles
- readable body text
- balanced labels, captions, metadata, and helper text
- avoid cramped text blocks

### Spacing
- use a consistent spacing rhythm
- allow content to breathe
- avoid cluttered layouts
- avoid over-compressed forms and tables

---

## Shared layout expectations

- pages should have clear structure
- headers should feel intentional
- sections should be visually grouped
- important actions should be easy to find
- content width should be controlled for readability
- responsiveness should be handled deliberately, not as an afterthought

---

## Shared component expectations

The app should use shared/reusable styling or components for:

- page headers
- buttons
- forms
- filter bars
- cards
- tables
- badges/chips
- empty states
- alerts/feedback
- modals/dropdowns if used
- navigation

Do not create a different design language on every page.

---

## Buttons

- primary actions should be visually clear
- secondary actions should support without competing
- destructive actions should be recognizable but not overly aggressive
- avoid too many equal-weight buttons in the same area

---

## Forms

- labels must be clear
- fields should be comfortably spaced
- validation states should be understandable
- forms should not look like raw backend forms
- grouped fields should feel intentional and readable

---

## Filters and search

- search/filter controls should be visually consistent across relevant pages
- filters should be easy to scan and use
- avoid clutter
- selected states should be clear
- reset/clear behavior should be simple

---

## Cards and content blocks

- cards should feel refined, not heavy
- avoid excessive borders and noisy shadows
- use spacing and hierarchy more than decoration
- cards should help readability and grouping

---

## Tables and lists

- tables should remain readable and elegant
- sorting behavior should be clear
- avoid heavy grid-like styling
- mobile fallback should remain usable
- rows should expose the right actions without becoming cluttered

---

## Empty, loading, and feedback states

### Empty states
- should explain what is missing
- should feel friendly
- should include a next action when useful

### Loading states
- should feel intentional
- avoid jarring layout shifts if possible

### Success/error feedback
- should be visible
- should use clear language
- should not feel overly technical

---

## User-facing vs admin styling

### User-facing pages
These should receive the highest level of polish:
- cellar
- wine catalog
- wine detail
- meals
- weekly menu
- pairing experiences

### Admin pages
Can be more utilitarian, but should still:
- remain coherent with the app
- avoid generic bootstrap admin-template styling
- use the same spacing and component logic where practical

---

## Accessibility expectations

- do not rely on color alone
- maintain readable contrast
- labels should be explicit
- clickable states should be visually clear
- keyboard usability should remain reasonable where practical
- responsive layouts should not hide core functionality

---

## Definition of Done

The design system direction is successful when:
- Bootstrap is not used
- the app has a coherent visual language
- user-facing pages feel premium and modern
- admin pages remain clean and practical
- shared components/patterns reduce inconsistency across the app
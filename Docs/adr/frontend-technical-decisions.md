# ADR — Frontend Technical Decisions

## Status
Accepted

## Context

The project is a Wine Cellar MVP web application with an existing Vue frontend and an ASP.NET Core backend API.

Over time, the product direction became clearer:
- the frontend should feel premium, modern, and consumer-facing
- the app should work well on mobile
- the UI should not look like a generic admin dashboard or CRUD tool
- Bootstrap is no longer considered a good fit for the product direction
- the current problem is primarily UI quality and frontend coherence, not incompatibility with the backend

The project also already has an existing frontend codebase, so major framework migration cost matters.

---

## Decision

The frontend will:
- continue using **Vue 3**
- continue using **Vue Router**
- continue using **Pinia**
- continue using **Axios** for API communication
- adopt a **mobile-first UI approach**
- use **Tailwind CSS** as the main styling system
- avoid Bootstrap entirely
- avoid migration to Nuxt at this stage

Optional supporting libraries may be introduced only if clearly justified, especially for accessible UI primitives:
- Headless UI
- Radix Vue
- similar lightweight headless component libraries

---

## Rationale

### Why keep Vue
- the project already uses Vue
- the current issue is design quality, not the core frontend framework
- keeping Vue avoids unnecessary migration churn
- it allows focus on redesigning the UI and UX rather than rebuilding the app shell

### Why not Nuxt
- Nuxt does not inherently conflict with the C# backend API
- however, moving to Nuxt now would introduce unnecessary architectural change during a UI redesign
- Nuxt would add routing/application-structure churn without directly solving the current design problems
- server-side rendering is not currently the primary need of the product

### Why Tailwind CSS
- Tailwind supports a strong mobile-first workflow
- it enables custom premium styling without Bootstrap-like visual constraints
- it works well for building a reusable internal design system
- it is better suited than Bootstrap to a refined wine/lifestyle-oriented product

### Why mobile-first
- the app should remain practical on phones and smaller screens
- mobile-first design helps force cleaner prioritization of layout and content hierarchy
- it improves responsiveness by design rather than as a late adaptation step

### Why no Bootstrap
- Bootstrap conflicts with the desired product feel
- it encourages a generic dashboard/CRUD look
- it makes it harder to establish a distinctive premium consumer-facing UI

---

## Consequences

### Positive
- less migration risk
- cleaner focus on UX and visual quality
- better fit for premium product direction
- better responsive design foundation
- easier to build a shared custom design system

### Tradeoffs
- some existing UI may need substantial refactoring
- Tailwind requires discipline to keep class usage maintainable
- some components may need to be rebuilt rather than taken from a ready-made UI kit

---

## Implementation guidance

- remove Bootstrap dependency and usage from the frontend
- replace Bootstrap classes and layout patterns systematically
- build or refine shared UI primitives for:
  - layout
  - buttons
  - forms
  - filters
  - cards
  - tables
  - empty states
  - navigation
- prioritize mobile-first layout decisions
- preserve current functionality while redesigning presentation and interaction quality

---

## Out of scope for this decision

This ADR does not decide:
- a full component library commitment beyond what is clearly needed
- SSR adoption
- public marketing site architecture
- native mobile app strategy
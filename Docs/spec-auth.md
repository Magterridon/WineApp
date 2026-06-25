# Wine Cellar App — Authentication Specification

## Goal

Provide simple, reliable authentication and role-aware access control for the app.

---

## Main flows

- register
- login
- authenticated session persistence
- logout
- protected routes
- admin-only route protection where needed

---

## Register

### Behavior
A user can create an account using:
- email
- password

### Requirements
- email is required
- email must be valid
- password is required
- password must meet minimum validation rules
- successful registration creates:
  - the user
  - the user's cellar

### UX expectations
- validation should be clear
- success/error states should be visible
- after success, the user should be guided into the app

---

## Login

### Behavior
A user can log in using:
- email
- password

### Requirements
- valid credentials return:
  - auth token
  - basic user information
  - role information if used by the frontend

### UX expectations
- invalid credentials show clear feedback
- successful login should redirect to an appropriate authenticated page

---

## Logout

### Behavior
- clears local auth state
- removes stored token/session data
- returns user to public/auth flow as appropriate

---

## Route protection

### Protected pages
Authenticated routes should require login.

### Admin pages
Admin pages must require:
- authenticated user
- admin authorization

Non-admin users must not be able to access admin pages or admin APIs.

---

## Session handling

- auth token should be attached to protected API requests
- auth state should survive refresh if current architecture supports persisted auth
- expired/invalid auth should fail gracefully and redirect as needed

---

## Authorization expectations

### Standard user
Can access:
- wines
- meals
- own cellar
- pairing visibility
- weekly menu if available

### Admin user
Can additionally access:
- wine backoffice
- pairing rule management
- other protected admin-only features

---

## Definition of Done

This feature is successful when:
- register works
- login works
- logout works
- cellar is created for new users
- protected routes work
- admin-only routes/pages are protected correctly
- auth failures are handled clearly
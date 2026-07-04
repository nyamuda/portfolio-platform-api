# Backend Implementation Guardrails

## Purpose

This document defines how backend work should be added to this API so the project stays consistent, predictable, and easy to maintain.

The API should be built by copying proven backend patterns first, then making only the changes that are required by the portfolio domain.

This is not a greenfield API experiment. Most first-pass backend work should feel like:

1. find the closest existing backend feature;
2. copy the same structure;
3. change only the domain-specific lines that must change;
4. improve comments or naming only when the improvement is clearly useful.

## Core Rule: Copy First, Adapt Only When Necessary

Do not rename, reshape, or redesign existing patterns just because another approach might also be valid.

If the existing backend already has a working pattern for a feature, the default is to copy that pattern almost exactly.

Only adapt when:

- the portfolio domain genuinely needs a different model or field;
- a copied name would be misleading in the new domain;
- the old code contains a known issue that should not be carried forward;
- the change was explicitly discussed and approved.

Avoid making independent architecture choices during the first port. The goal is speed, consistency, and confidence.

## Non-Negotiable Conventions

Use these conventions unless there is a clear, approved reason to do otherwise:

- Model primary keys use `int`.
- Core account model is named `User`.
- Public portfolio identity is named `Profile`.
- User work items are named plainly, for example `Project`, `BlogPost`, `CaseStudy`, and `Testimonial`.
- Controllers stay thin.
- Business rules live in services.
- Services are registered through interfaces.
- Request and response contracts use DTOs.
- DTOs should not expose password hashes or internal implementation details.
- Simple model constraints such as string lengths and single-entity unique indexes should live on the model classes.
- `ApplicationDbContext` should focus on relationships, delete behavior, TPH/discriminator configuration, join tables, and composite/query-performance indexes that need explanation.
- Enums should serialize as camelCase strings.
- Public reads should use efficient projections rather than loading full entity graphs.
- Error responses should use the shared `ErrorResponse` pattern.
- Expected conflicts should use `ConflictException` where appropriate.

## Feature Porting Checklist

Before writing code for a feature:

1. Find the closest matching feature in the existing backend.
2. Read the model, DTOs, controller, service interface, service implementation, and DbContext configuration.
3. Note the id types, route names, DTO names, service method names, exception types, and mapping style.
4. Copy the structure first.
5. Rename only when the existing name would be wrong or confusing in the portfolio domain.
6. Keep the same layering: controller -> service abstraction -> service implementation -> DTO/model.
7. Prefer projections for read endpoints.
8. Preserve useful inner comments.
9. Improve inner comments when they make the code easier to maintain.
10. Do not mention the source application in code comments.
11. Run build and formatting checks when the installed SDK supports the target framework.

## Documentation Rules

Documentation should follow the existing backend style.

### Service Interfaces

Service interface methods should contain the full XML documentation:

- summary;
- parameter descriptions;
- return description where useful;
- exception descriptions where useful;
- remarks when the method has important workflow or security behavior.

The interface is the contract. It should explain what the method does and what callers should expect.

### Service Implementations

Service implementation methods should normally use:

```csharp
/// <inheritdoc/>
```

Do not duplicate the full XML documentation in the implementation unless the implementation has extra behavior that is not obvious from the interface.

### Inner Comments

Inner comments are important and should be kept when they explain the flow of a method.

Good inner comments explain steps such as:

- checking whether a user exists;
- validating ownership;
- checking uniqueness;
- hashing a password;
- creating a token;
- building an entity;
- saving changes;
- sending or storing a notification/message;
- intentionally returning early.

Avoid empty comments that only repeat a single line of code.

Better:

```csharp
// Check if another account already uses the requested username
bool usernameAlreadyInUse = await _context.Users.AnyAsync(...);
```

Not useful:

```csharp
// Set username
user.Username = dto.Username;
```

## Naming Rules

Use simple domain names. Avoid over-qualified names when the namespace already gives context.

Good names:

- `User`
- `Profile`
- `Project`
- `BlogPost`
- `CaseStudy`
- `ContactMessage`

Avoid names like:

- `ApplicationUser`
- `ProfessionalProfile`
- `PortfolioProject`

Those names add noise without improving clarity.

## When Improvement Is Allowed

Improvements are allowed, but they must be deliberate and small during the first port.

Good first-pass improvements:

- clearer XML documentation on interfaces;
- better inner comments for multi-step methods;
- efficient projections instead of unnecessary includes;
- safer validation where the existing behavior is clearly incomplete;
- fixing obvious bugs while preserving the same structure.

Avoid first-pass changes like:

- changing id types;
- replacing controller error-handling style;
- changing auth response shapes;
- introducing a new service pattern;
- renaming familiar models without a domain reason;
- adding a new architecture layer.

## Final Rule

When in doubt, copy the existing pattern and make fewer changes.

The portfolio domain can become richer over time, but the foundation should be familiar, boring, and reliable.



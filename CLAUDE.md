# Eshava.DomainDrivenDesign — Repository Notes

Base implementation of the domain-driven design approach: one set of abstract classes per layer,
meant to standardise code and enforce a defined project structure. Published as the NuGet package
**`Eshava.DomainDrivenDesign`**.

**Conventions:** documentation, code and commit messages are written in English. Line endings are
pinned through `.gitattributes` — anything that may run on Linux must be checked out with LF.

## Layout

| Project | Content |
|---|---|
| `Eshava.DomainDrivenDesign` | The library itself — abstract base classes for all four layers. |
| `Eshava.Example.Domain` · `.Application` · `.Infrastructure` · `.Api` | A sample API demonstrating the approach. The infrastructure project also contains the SQL script for the sample tables. |

The sample is documentation, not a scratch pad. It is what readers of the README look at, so it
has to stay consistent with the library. There is no test project in this repository.

## Architectural Rules

These are the constraints the library exists to enforce. Breaking one of them in the sample is
as much a defect as breaking it in the library.

**Layer access is one-directional and narrow.**

```
Api ──────────► Application
Application ──► Domain
Infrastructure ► Application · Domain
```

* **Domain** holds only domain models, value objects and C# enums. Organised by domain, then by
  feature or model name. Three model kinds: standalone, aggregate, child — a child model can be
  an aggregate itself.
* **Application** holds the business logic that cannot live inside a domain model, organised
  into use cases and split into Queries and Commands. **No direct access to external resources**
  — no database, no API, no file system. All of it goes through `InfrastructureProviderService`
  interfaces, which are declared here and implemented in the infrastructure layer.
* **Infrastructure** encapsulates every external resource. Domain models cross this boundary
  **as a unit** — the layer is responsible for the integrity of the aggregate. The
  `InfrastructureProviderService` is the only interface towards the application layer and
  orchestrates the individual repositories of an aggregate.
* **Api** provides endpoints only, and reaches the application layer exclusively. A UI may take
  its place.

**The provider layer exists on the reading side as well.** A query repository answers with the
data of its own aggregate and nothing more. As soon as a dto has to be enriched from somewhere
else — file contents from a storage, a value another repository owns — that assembly belongs into
the `InfrastructureProviderService`, because the storage is a repository itself and a repository
never reaches into another one. Collapsing a query provider into its query repository saves one
class and takes that possibility away, so the two stay separate **even where the provider only
forwards**.

**Domain models validate themselves.** Every model carries validation rules that keep its own
state valid; an aggregate may additionally hold rules spanning its child models. Validation
that needs information from outside the model — a reference to another domain model, for
instance — belongs into the use case, never into the model.

**Value objects are immutable.** Parameterised constructor, no changes after creation; changing
any part means recreating the whole object. They are used as properties of domain models.
Nested value objects are not supported. The infrastructure layer distinguishes
`CompositionValueObject` (properties map onto data model properties) from `UnitValueObject` (the
object as a whole maps onto one property).

**Every model action raises a model-internal event.** Standard events are `created`, `changed`
and `deactivated`; further ones can be added. Generation is on by default and can be switched
off per model, with extra settings on aggregates for child model events. Events are distributed
on save: the `InfrastructureProviderService` collects them and converts the model events into
domain events.

**Entity Framework is deliberately not used, and its introduction is not planned.** The
infrastructure base classes target SQL databases directly, via `Eshava.Storm`. The reasoning:
EF only pays off if its models never leave the infrastructure layer, which this structure would
have to guarantee separately anyway. Other database types are not implemented.

## Dependencies

Consumes `Eshava.Core` and, in the infrastructure layer, `Eshava.Storm` and `Eshava.Storm.Linq`
— all as NuGet packages, never as project references across repository boundaries.

`Eshava.DomainDrivenDesign.CodeAnalysis` generates code against this library. **A change to the
abstract base classes usually forces a change to the generator templates there.** Check that
repository before altering a signature.

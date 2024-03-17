### Task 1

- Divided models into write models, read models and db models. These models should live separately in order to have control over what is shared with the outside world. The second thing is that write models (used in `POST`s) have their ids filled by the infrastructure so the client shouldn't be mislead that they have to provide the id.
- Mapping taken away from the controllers (`ToDbModel`, `FromDbModel`).
- Created generic `CosmosRepository` to encapsulate access to the database (introduced `ICosmosEntity`).
- Premium computation moved to a separate service.
- Added Swagger documentation.
- Changed some of the endpoints to make the API more RESTfull: `404 NotFound` when the resource is missing, `201 Created` (with link to the resource in headers and the resource itself in body) when new resource is created, `204 NoContent` when no resource is found and on deletion.
- Dropped `Async` suffix from controller methods because `CreatedAtAction` has problem generating the reponse if method name end with `Async` (weird, right?).
- `Get` and `GetAll` method so there's no ambiguity for method to generate header link when using `CreatedAtAction`.
- Used `ActionResult<>` for better communication and documentation.
- Null checks in controllers redundant - the DI takes care of that.
- Dry premium computation moved to `/premium` (was conflicting with the other `GET` endpoint).
- TODO: improve error handling by logging and fine tuning API responses.

### Task 2

- Simple validations done by implementing `IValidatableObject` interface in write models.
- Claim against Cover validation implemented in the controller.
- TODO: Consider using `FluentValidations` to gather all validations in one place.

### Task 3
- Delegated saving audits to a background service that persists them periodically and before that keeps them in memory.
- Implemented basic failsafe to flush pending audits on unexpected service crash.
- TODO: implement rollback mechanism to remove audit in case Cosmos operation fails. On exception from Cosmos there should be audit deletion request in the catch block to assure better data consistency.

### Task 4
- Implemented 1 full integration test for claims: create, delete, check if deleted.
- TODO: similar test for covers, test `BadRequst` in case of invalid data.

### Task 5
- First told Github Copilot to write relevant tests based on the description and then implemented changes in the `ComputePremium` (guess it's AI TDD).

### Other
- You do know that I'd never commit straight to `master` in the real life, right?
- Used `record` with either constructor or `init` initialization to have immutable data structure.
- Cleaned startup up: made whole thing `async`, removed blocking database initialization and moved it just before the app start.
# Validation from the EF model

Status: proposed (2026-11-01). Basics only.

Forms on EF Core resources validate only through attributes and MVC's implicit required check on non-nullable types. Rules configured in the `DbContext`, such as `HasMaxLength(100)`, aren't checked, so a value that breaks them passes the form and fails in `SaveChanges` (SQL Server) or is stored anyway (SQLite).

The EF data source already resolves each form field to its EF `IProperty` (`EfCoreFormFieldMetadata.FindProperty`), and the property gives `GetMaxLength()`, `IsNullable` and, for decimals, `GetPrecision()` and `GetScale()`. Nothing reads them for validation today.

Idea: turn these rules into form errors, and possibly input attributes such as `maxlength`, for forms bound to the entity and for wrapped form models ([custom create and edit models on EF Core resources](efcore-form-models.md)).

To be filled out before implementing.

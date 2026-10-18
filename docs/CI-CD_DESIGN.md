# CI/CD Pipeline — Idea and Key Decisions

## Key Decisions

### Things that should be created before running the pipeline

1. **Resource Group** — to contain all the Azure resources.
2. **Application Registration for the pipeline** — the pipeline will use this application to authenticate to Azure using OIDC and be configured for federated identity credential for GitHub.
3. **Group for Azure SQL Entra administrators** — this group will contain my account and the pipeline application, so both of them can act as Entra administrators of the Azure SQL Server.
4. **Permissions for the pipeline application:**
   - Azure RBAC role: `Contributor`, to provision and manage Azure resources.
   - Entra ID role: `Privileged Role Administrator`, to assign the Directory Readers role to the SQL Server identity.

## Pipeline

The pipeline aims to achieve a ZERO-TOUCH DEPLOYMENT.
The pipeline is responsible for building, testing, publishing, and deploying the application, as well as provisioning the required Azure resources and executing database migrations.

### 1. Authentication

The pipeline needs to authenticate to Azure before it can provision resources.

I am using an Application Registration for the pipeline and configuring OIDC authentication through a federated identity credential. This allows GitHub Actions to authenticate to Azure without without storing credentials.

### 2. Provisioning Azure resources

The pipeline needs permissions to create and manage Azure resources, such as the SQL Server, SQL Database, managed identity, and Azure Container App.

That is why I assign the `Contributor` role to the pipeline application at the Resource Group scope.

### 3. Assigning Directory Readers to the SQL Server identity

When the SQL Server is created, it is configured to have a system-assigned identity.

I need to assign the `Directory Readers` Entra role to this identity so that Azure SQL can query Microsoft Entra Directory and resolve identities when creating database users(Logins).

Because the pipeline is responsible for assigning this role automatically, it needs the sufficient Entra role. My current decision is to assign the `Privileged Role Administrator` role to the pipeline application.

### 4. Executing database migrations

The pipeline needs to execute EF Core migrations against the Azure SQL Database.

To do this, I configure an Entra administrator for the SQL Server using a group that contains my account and the pipeline application. This allows me to access the database manually when needed and allows the pipeline to execute migrations.

The group is configured as the Entra administrator of the Azure SQL Server.

---

## Azure SQL Server

### 1. Entra authentication

The SQL Server will use Microsoft Entra authentication.

I want to use Entra identities to authenticate to the database instead of storing any secrets

### 2. Entra administrator

I am using a group as the Entra administrator of the SQL Server instead of assigning an individual account directly.

The group contains my account and the pipeline application. This allows me to manage the database manually through the Azure Portal or other tools, while the pipeline can also execute the required SQL commands.

### 3. Directory Readers role

The SQL Server needs the `Directory Readers` role because the pipeline will create a database user for the Azure Container App's managed identity.

When I create a login/user for an identity in the database, Azure SQL needs to be able to check and resolve that identity in Microsoft Entra ID.

For example, I want to create a database user for the application's managed identity and grant it the required database roles, such as `db_datareader` and `db_datawriter`.

That why I assign the Directory Readers role to the SQL Server's managed identity, allowing SQL Server to query the directory when resolving Entra identities.

---

## Running Migrations

I am using an EF Core migration bundle (`efbundle`) to execute database migrations.

The bundle is generated during the build and test stage, then uploaded as a pipeline artifact.

In the migration job, I download the bundle and execute it against the Azure SQL Database.

The pipeline can execute the migrations because its application is a member of the group configured as the SQL Server's Entra administrator.

---

## Creating a User-Assigned Managed Identity

I am creating a user-assigned managed identity for the Azure Container App.

The reason I use a user-assigned identity is that I want to create its database user and assign the required database roles before deploying the application.

The identity is created in a separate pipeline job, and its information is passed to the following jobs as outputs.

Once the identity is created, the pipeline creates its database user and grants it the required permissions.

After that, the identity is passed to the application deployment so the Azure Container App can use it directy to authenticate to the database.

### Why create the identity before deploying the application?

My goal is to make the application deployment the last step of the pipeline and have the application ready to interact with the database as soon as it is deployed.

If I used a system-assigned managed identity, I would need to deploy the Azure Container App first, retrieve its identity, create its database user, and grant the required database roles afterward.

That would introduce additional steps after the application deployment before the application could access the database successfully.

By using a user-assigned managed identity, I can create the identity independently, configure its database permissions, and then attach it to the application during deployment.

---

## Security and Credentials Management

### Protecting Sensitive Information

**Never commit secrets to source control.** This includes:
- Azure Cosmos DB connection strings
- Account keys (primary/secondary)
- Resource tokens
- Any authentication credentials

### Best Practices

#### Use Environment Variables
Store sensitive configuration in environment variables or secure configuration providers:
```bash
COSMOS_ENDPOINT=https://your-account.documents.azure.com:443/
COSMOS_KEY=<stored-securely>
```

#### Leverage Azure Key Vault
For production environments:
- Store connection strings in **Azure Key Vault**
- Use **Managed Identity** to authenticate your application
- Reference secrets at runtime, never hardcode

#### Use .gitignore
Ensure your `.gitignore` includes:
```
.env
appsettings.json
appsettings.Development.json
local.settings.json
```

#### Development with Emulator
For local development, use the [Azure Cosmos DB Emulator](https://learn.microsoft.com/azure/cosmos-db/emulator):
- Well-known emulator endpoint and key (safe for local use only)
- No cloud costs
- Never use emulator credentials in production

### Additional Resources
- [Azure Key Vault Documentation](https://learn.microsoft.com/azure/key-vault/)
- [Managed Identity Overview](https://learn.microsoft.com/azure/active-directory/managed-identities-azure-resources/)
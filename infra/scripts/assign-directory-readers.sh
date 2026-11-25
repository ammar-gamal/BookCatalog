#!/usr/bin/env bash
# Assigns the Entra ID "Directory Readers" role to a principal.
# Usage: ./assign-directory-readers.sh <principal-object-id>

set -u

PRINCIPAL_ID="${1:-}"

if [ -z "$PRINCIPAL_ID" ]; then
  echo "Usage: $0 <principal-object-id>" >&2
  exit 2
fi


DIRECTORY_READERS_ROLE_ID="88d8e3e3-8f55-4a1e-953a-9b9898b8876b"
GRAPH_URL="https://graph.microsoft.com/v1.0/roleManagement/directory/roleAssignments"

# Skip if already assigned (keeps reruns idempotent)
if ! existing=$(az rest --method GET \
  --uri "$GRAPH_URL?\$filter=principalId eq '$PRINCIPAL_ID' and roleDefinitionId eq '$DIRECTORY_READERS_ROLE_ID'" \
  --query "length(value)" -o tsv); then
  echo "ERROR: Failed to check existing role assignments." >&2
  exit 1
fi

if [ "$existing" != "0" ]; then
  echo "Directory Readers already assigned to $PRINCIPAL_ID"
  exit 0
fi

# Retry because a newly created identity can take a moment to replicate in Entra ID
for attempt in 1 2 3 4 5 6; do
  if az rest --method POST \
    --uri "$GRAPH_URL" \
    --headers "Content-Type=application/json" \
    --body "{\"principalId\":\"$PRINCIPAL_ID\",\"roleDefinitionId\":\"$DIRECTORY_READERS_ROLE_ID\",\"directoryScopeId\":\"/\"}"; then
    echo "Directory Readers assigned to $PRINCIPAL_ID"
    exit 0
  fi
  echo "Attempt $attempt failed, retrying in 20s..."
  sleep 20
done

echo "Failed to assign Directory Readers role"
exit 1
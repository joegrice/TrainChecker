# GitHub Actions Setup for Docker Hub

This document explains how to set up the GitHub Actions workflow to automatically build and push Docker images to Docker Hub when you create a release.

## Prerequisites

1. A Docker Hub account with the repository `joegrice/trainchecker`
2. A GitHub repository with this code

## Setup Instructions

### 1. Create Docker Hub Access Token

1. Log in to [Docker Hub](https://hub.docker.com/)
2. Go to **Account Settings** → **Security**
3. Click **New Access Token**
4. Give it a name (e.g., "GitHub Actions")
5. Select **Read, Write, Delete** permissions
6. Copy the generated token (you won't see it again)

### 2. Add GitHub Secrets

In your GitHub repository:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret** and add:
   - **Name**: `DOCKERHUB_USERNAME`
   - **Value**: Your Docker Hub username (`joegrice`)
3. Click **New repository secret** and add:
   - **Name**: `DOCKERHUB_TOKEN`
   - **Value**: The access token you created in step 1

### 3. How the Workflow Works

The workflow (`.github/workflows/docker-release.yml`) will:

- **Trigger**: Automatically run when you publish a GitHub release
- **Build**: Create a multi-platform Docker image (AMD64 and ARM64)
- **Tag**: Use the release version as the Docker tag (e.g., `v1.0.0`)
- **Push**: Upload to `joegrice/trainchecker:v1.0.0` and `joegrice/trainchecker:latest`

### 4. Creating a Release

To trigger the workflow:

1. Go to your GitHub repository
2. Click **Releases** → **Create a new release**
3. Choose or create a tag (e.g., `v1.0.0`)
4. Fill in the release title and description
5. Click **Publish release**

The workflow will automatically:
- Build the Docker image
- Push it to Docker Hub with the release tag
- Also tag it as `latest`

### 5. Using the Published Image

After the workflow completes, you can use your image:

```bash
# Pull the latest version
docker pull joegrice/trainchecker:latest

# Pull a specific version
docker pull joegrice/trainchecker:v1.0.0

# Run the container
docker run -d \
  --name trainchecker \
  -p 8080:8080 \
  --env-file .env \
  joegrice/trainchecker:latest
```

### 6. Updating docker-compose.yml for Published Image

You can also update your `docker-compose.yml` to use the published image instead of building locally:

```yaml
version: '3.8'

services:
  trainchecker:
    image: joegrice/trainchecker:latest  # Use published image
    # Remove the 'build' section
    ports:
      - "8080:8080"
    env_file:
      - .env
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
    restart: unless-stopped
```

## Troubleshooting

- **Workflow fails**: Check that your Docker Hub credentials are correctly set in GitHub secrets
- **Permission denied**: Ensure your Docker Hub token has write permissions
- **Build fails**: Check the workflow logs in the **Actions** tab of your GitHub repository

## Workflow Features

- **Multi-platform builds**: Supports both AMD64 and ARM64 architectures
- **Caching**: Uses GitHub Actions cache to speed up builds
- **Automatic tagging**: Uses release version and latest tags
- **Security**: Uses secrets for Docker Hub authentication
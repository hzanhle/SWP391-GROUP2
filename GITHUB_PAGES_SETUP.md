# 🚀 GitHub Pages Setup Guide

## Quick Setup

### 1. Enable GitHub Pages in Repository Settings

1. Go to your repository on GitHub
2. Navigate to **Settings** → **Pages**
3. Under **Source**, select:
   - **Source**: `GitHub Actions`
4. Save the settings

### 2. Configure Repository Secrets (Optional)

If you want to use a different API URL for production builds, add a repository secret:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**
3. Name: `VITE_API_URL`
4. Value: Your production API URL (e.g., `https://your-api-domain.com`)
5. Click **Add secret**

> **Note**: If no secret is set, the workflow will default to `http://localhost:5000`

### 3. Push Changes

The GitHub Actions workflow will automatically deploy when you push to `main` or `master` branch with changes in the `Frontend/` directory.

```bash
git add .
git commit -m "Setup GitHub Pages deployment"
git push origin main
```

### 4. Check Deployment Status

1. Go to **Actions** tab in your repository
2. Monitor the workflow run
3. Once complete, your site will be available at:
   ```
   https://yourusername.github.io/SWP391-GROUP2/
   ```

### 5. Update README with Live URL

After deployment, update the README.md to include your actual GitHub Pages URL in the deployment section.

## Manual Deployment

If you want to deploy manually:

```bash
cd "Frontend/EV Station-based Rental System"
npm run build

# The dist/ folder contains the built files
# You can deploy this folder to any static hosting service
```

## Troubleshooting

### Build Fails
- Check GitHub Actions logs for errors
- Ensure `package-lock.json` is committed
- Verify Node.js version compatibility

### 404 Errors on Routes
- Ensure `.nojekyll` file exists in the dist folder (already included)
- Verify `base` path in `vite.config.js` matches your repository name

### API Calls Not Working
- Update `VITE_API_URL` in repository secrets
- Ensure your backend API is accessible from the internet
- Check CORS settings on your API Gateway

## Notes

- GitHub Pages only hosts static files (Frontend only)
- Backend services need to be deployed separately (Azure, AWS, etc.)
- The workflow triggers only when `Frontend/` files change
- Build artifacts are stored temporarily by GitHub


## Using Git Tags and Branches for Documentation Versioning

Tanka Docs is designed to work seamlessly with Git, leveraging its powerful branching and tagging features to manage and generate versioned documentation. Understanding how to use tags and branches effectively is key to maintaining a clear and accessible history of your documentation as your project evolves.

### Git Tags: Capturing Specific Versions

Git tags are typically used to mark specific, stable releases of your codebase, and they serve the same purpose for your documentation. When you release a new version of your project (e.g., v1.0.0, v2.1.0), you should also tag the corresponding state of your documentation.

**How it works with Tanka Docs:**

-   **Point-in-Time Snapshots:** A Git tag creates a snapshot of your documentation content at a specific point in time. This means you can always go back and build the documentation exactly as it was for that release.
-   **Configuration:** In your `tanka-docs.yml` configuration file, you can specify which Git tags Tanka Docs should process. This allows you to generate documentation for multiple released versions (e.g., v1.0, v1.1, v2.0).
-   **User Access:** Users can then typically select which version of the documentation they want to view through the UI provided by Tanka Docs, ensuring they see information relevant to the version of the product they are using.

**Example Workflow for Tags:**

1.  Finalize the documentation for your upcoming release (e.g., v1.2.0).
2.  Commit all documentation changes to your Git repository.
3.  Create a Git tag for this version: `git tag -a v1.2.0 -m "Documentation for version 1.2.0"`
4.  Push the tag to your remote repository: `git push origin v1.2.0`
5.  Configure Tanka Docs (in `tanka-docs.yml`) to include `v1.2.0` (or a pattern like `1.2.*`) in the list of tags to build.

### Git Branches: Managing Ongoing Development and Future Releases

Git branches are essential for managing changes to your documentation, especially for ongoing development of new features or future versions.

**Common Branching Strategies:**

1.  **Main/Master Branch (Latest Stable/Published):**
    *   Often represents the most current, published version of your documentation.
    *   Tags are typically created from this branch for releases.

2.  **Development Branch (e.g., `develop`, `next-version`):**
    *   Used for preparing the documentation for the next upcoming release.
    *   All new documentation features, updates for new product features, and major rewrites would happen here.
    *   Once the next version is ready for release, this branch can be merged into `main`/`master`, and a new tag is created.

3.  **Feature Branches (e.g., `feature/new-api-docs`, `fix/typo-in-install-guide`):**
    *   Created from a development branch (or main, for hotfixes) to work on specific documentation tasks or sections.
    *   This isolates changes and allows for review (e.g., via Pull Requests) before merging them into the primary development line.

**How it works with Tanka Docs:**

-   **Building Pre-Releases:** You can configure Tanka Docs to build documentation from specific branches, such as your `develop` branch. This allows you to deploy a "preview" or "edge" version of your documentation, giving stakeholders a chance to review upcoming changes.
-   **Isolation:** Working on documentation for a new, unreleased version in a separate branch ensures that your publicly visible, tagged versions remain stable and unchanged until you're ready to release the new version.
-   **Configuration:** The `branches` section in `tanka-docs.yml` allows you to specify which branches Tanka Docs should process. You might have `master` for the latest stable release and `develop` for the upcoming version.

**Example Workflow for Branches:**

1.  Create a `develop` branch if you don't have one: `git checkout -b develop`
2.  For a new documentation effort, create a feature branch from `develop`: `git checkout -b feature/document-new-module develop`
3.  Make your documentation changes on the feature branch.
4.  Commit and push the feature branch.
5.  Create a Pull Request to merge `feature/document-new-module` into `develop`.
6.  Once reviewed and merged, the `develop` branch now contains these updates. Tanka Docs can be configured to build this branch for a preview site.
7.  When ready for a new release, merge `develop` into `main`/`master` and create a tag.

By combining Git tags for stable releases and branches for development, you can maintain a robust and flexible versioning strategy for your documentation, ensuring that users always have access to accurate and relevant information for their needs. Tanka Docs is designed to integrate with this workflow by allowing you to define which tags and branches are used to generate your documentation site.
```

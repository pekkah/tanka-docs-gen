## Publishing to GitHub Pages

As the generated document site is static it only requires
a web server capable of serving static file content. This
makes publishing the site to GitHub Pages easy.

Following is the recommended approach allowing you to keep
your main branch clean of the generated static content. In
this approach the static content is generated into a dedicated
`gh-pages` branch by creating a orphaned branch from master.


1. Create branch

Create `gh-pages` branch orphaned from master

> Create a new orphan branch, named <new_branch>, started from
> <start_point> and switch to it. The first commit made on this
> new branch will have no parents and it will be the root of a new
> history totally disconnected from all the other branches and commits.

```bash
git checkout --orphan gh-pages
git reset --hard
# If there's any Git ignored content in the folder you might
# want to clean it up before continuing.
git commit --allow-empty -m "Initialize"
git push origin gh-pages
git checkout master
```

2. Checkout `gh-pages` branch as worktree into output folder

```bash
git worktree add -B gh-pages docs-html origin/gh-pages
```

3. Generate docs to output folder

```bash
generate-docs
```

4. Push your output folder to gh-pages branch

```bash
cd docs-html
git push
```
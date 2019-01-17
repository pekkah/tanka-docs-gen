## Publishing to GitHub Pages

### 1. Create branch

Create gh-pages branch orphaned from master

> Orphaned branches are separated from master history

```bash
git checkout --orphan gh-pages
git reset --hard
git commit --allow-empty -m "Initialize"
git push upstream gh-pages
git checkout master
```

### 2. Create gh-pages branch worktree as output folder

```bash
git worktree add -B gh-pages docs-html origin/gh-pages
```

### 3. Generate docs to output folder

```bash
dotnet generate-docs
```

### 4. Push your output folder to gh-pages branch

```bash
cd docs-html
git push
```
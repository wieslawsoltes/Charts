# Contributor Guidelines and Agent Standards

This document establishes the official engineering and content standards for all contributors (including human developers and artificial intelligence agents) to the ProCharts repository.

## Professional Content Standards

### 1. Style and Tone
- All writing must be formal, exact, mathematically sound, and technically rigorous.
- Avoid casual language, colloquialisms, and conversational fillers.
- Present explanations clearly, focusing on performance characteristics, coordinate mathematics, and architectural patterns.

### 2. Strict Emoji Prohibition
- Emojis are strictly prohibited in all documentation, reports, outputs, pull request summaries, commit messages, code comments, and any other files, unless specifically asked or required by the user.
- Emphasize maintaining a highly professional, technical, and precise tone for all technical artifacts, keeping with a state-of-the-art vector charting library.
- Examples of prohibited emojis: 🚀, 🛠️, 🏛️, 💻, 🛡️, 📊, 📈, 🧪, and similar graphics.
- Technical symbols and standard markdown/LaTeX notations (e.g., standard formula rendering) are fully supported and should be utilized where appropriate.

## Version Control and Commit Conventions

### 1. Branch Naming
- Branch names should follow clear and professional naming conventions (e.g., `feature/description-of-feature`, `bugfix/description-of-bug`).
- Do not use emojis, unicode shorthand, or informal tags in branch names.

### 2. Commit Messages
- Use the Conventional Commits specification (e.g., `feat: add support for logarithmic scaling`, `docs: clarify CoordinateTransform mapping equation`, `fix: resolve high-DPI scaling issue on WebAssembly`).
- Do not include emojis under any circumstances in commit headers or body text.
- Maintain a concise, descriptive, and imperative tone.

### 3. Pull Request Summaries
- Pull request titles and descriptions must be structured, technical, and professional.
- Focus on describing:
  - The precise problem or feature addressed.
  - Architectural changes introduced.
  - Verification and testing steps performed.
  - Side effects or backwards compatibility implications.

## Code and Comment Integrity

- **Code Comments**: Keep inline comments focused purely on technical rationale, complex mathematical logic, or framework-specific nuances.
- **XML Documentation**: Every public class, control, property, and method should possess clear XML documentation tags (`<summary>`, `<param>`, `<returns>`) in a formal voice.
- **Preserve Existing Documentation**: Do not remove existing comments or docstrings that are unrelated to your current changes.

# Contribution Guidelines

Please take a moment to review this Contributing Guide before you get started. It outlines the process for contributing to Payment Gateway and provides important information on how to submit your contributions.

## Table of contents

- Coding Guidelines
- Branch Naming
- Commit Messages
- Submitting Pull Requests
- Testing
- Documentation
- Getting Help

## Coding Guidelines

- We follow the same coding guidelines used by dotnet/aspnetcore.
- We use .editorconfig. Some of the coding guidelines are enforced by the .editorconfig file.
- We avoid overly complex or obtuse logic, code should mostly document itself. Before you write a code comment think if you can make it describe itself first.
- This is shared software. Consider the people who will read your code, and make it look nice for them.

The formatting is achieved via **CSharpier**. We have configured the tool as prerequisite for local installation.
To set up CSharpier locally:
1. Run `dotnet tool restore` to install the tool. Skip, if completed during the initial setup.
2. Configure the CSharpier plugin for your preferred IDE and enable **“Format on Save”**.

Changes to the `.editorconfig` file may impact the entire codebase. To ensure consistency, please discuss any modifications with the *team* before proceeding.

## Architecture Decisions

### Vertical Slice + feature namespace convention

Payments feature code is organized by vertical slices under `Features/Payments`.

To keep feature usage clean and avoid namespace proliferation, slice files intentionally share one namespace:

`PaymentGateway.Api.Features.Payments`

This means folder names and namespaces do not always match. That is expected in this project and is explicitly suppressed with targeted pragma directives in feature files.

When adding new files under `Features/Payments`, follow the same convention unless the team agrees on a broader namespace policy change.

## Branch Naming

Use the following conventions for branch names:
- `dev-name/JIRA-123`
- `dev-name/pipeline-maintenance`

## Commit Messages

A commit:
- should ideally address one issue
- should be a self-contained change to the code base
- must not lump together unrelated changes

Use imperative language in commit messages (*e.g.*, “Add feature” instead of “Added feature”).
- Keep messages concise and descriptive.
- Examples:
- `Fix bug in invoice calculation`
- `Add unit tests for payment processing`
- `Refactor code for better readability`

## Submitting Pull Requests

1. Start by pulling the latest changes from `main` before creating your branch.
2. Add tests for any new code or changes where applicable.
3. Ensure all tests pass and the build shows no errors or warnings.
4. Make sure your code adheres to the project’s coding standards.
5. Submit your pull request to the `main` branch:
    - **Title**: Use a clear, descriptive title, ideally matching the JIRA ticket title (*e.g.*, `JIRA-123: Add new feature`). This helps simplify release tracking by aligning Git history with JIRA.
    - **Description**: Replace the prefilled pull request template. Include:
        - The JIRA ticket URL for easy reference.
        - A brief summary of the changes to provide reviewers with context.
6. Once all checks pass and feedback is addressed, PR can be completed by the *author*.

## Testing

Use the following format for test method names:

**`MethodUnderTest_StateUnderTest_ExpectedOutcome`**

- **Example**: `ProcessOrder_WithInvalidPayment_ShouldFail`

This naming convention ensures clarity and makes it easy to understand the purpose of the test at a glance.

Testing philosophy:
- Whenever you encounter a bug, add a (failing) test for it. Then fix the bug.
- Whenever you modify or add a feature, write a test for it!
- Make unit tests as atomic as possible. A unit test should run in the blink of an eye.

## Documentation

Good documentation is essential for the success of any project. We should improve our documentation whenever its possible by creating clear, concise, and easy-to-understand documentation.

## Getting Help

If you have any questions or need assistance, don’t hesitate to reach out! You can contact the product team or open an issue in our internal tracking system for support.

Thank you for your contributions, you’re awesome and wouldn’t be possible without you!
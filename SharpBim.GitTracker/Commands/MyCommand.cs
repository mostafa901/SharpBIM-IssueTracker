using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.VisualStudio.Text;

namespace SharpBim.GitTracker
{
    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            //    await VS.MessageBox.ShowWarningAsync("GenerateMvvm", "Button clicked");
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            var selection = docView?.TextView.TextViewLines;

            var snapshot = docView?.TextView.TextSnapshot;

            if (snapshot != null)
            {
                // Get the full text of the document
                var code = snapshot.GetText();

                // Parse the text using Roslyn
                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var root = syntaxTree.GetRoot();

                // Find all class declarations
                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var classNode in classDeclarations)
                {
                    // Get the class name
                    var className = classNode.Identifier.Text;

                    // Get the class members (fields, properties, methods)
                    var members = classNode.Members;

                    //  Console.WriteLine($"Class: {className}");
                    foreach (var member in members)
                    {
                        //   Console.WriteLine($"  Member: {member}");
                        if (member.Kind() == SyntaxKind.PropertyDeclaration)
                        {
                            var property = (PropertyDeclarationSyntax)member;

                            // Check if the property has a public accessibility modifier
                            var hasPublicAccess = property.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));

                            var hasGetterAndSetter = property.AccessorList?.Accessors
                    .Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration) && a.Body == null) == true &&
                    property.AccessorList?.Accessors
                    .Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration) && a.Body == null) == true;

                            // Output the result
                            if (hasPublicAccess && hasGetterAndSetter)
                            {
                                var updatedClass = classNode;

                                if (property.Initializer != null)
                                {
                                    // Get or create the constructor
                                    var constructor = classNode.Members
                                        .OfType<ConstructorDeclarationSyntax>()
                                        .FirstOrDefault();

                                    if (constructor == null)
                                    {
                                        // Create a new constructor if none exists
                                        constructor = SyntaxFactory.ConstructorDeclaration(classNode.Identifier)
                                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                            .WithBody(SyntaxFactory.Block());
                                    }
                                    // Add assignments to the constructor body

                                    var assignment = SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName(property.Identifier),
                                            property.Initializer.Value));

                                    var updatedConstructor = constructor.WithBody(constructor.Body.AddStatements([assignment]));
                                    // Update the class node
                                    updatedClass = classNode
                                      .AddMembers(updatedConstructor);

                                    root = root.ReplaceNode(classNode, updatedClass);
                                }

                                // Create the new getter and setter with custom logic
                                var newGetter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithBody(SyntaxFactory.Block(
                                        SyntaxFactory.SingletonList(
                                            SyntaxFactory.ReturnStatement(
                                                SyntaxFactory.ParseExpression($"GetValue<int>(nameof({property.Identifier.Text}))")))));

                                var newSetter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .WithBody(SyntaxFactory.Block(
                                        SyntaxFactory.SingletonList(
                                            SyntaxFactory.ExpressionStatement(
                                                SyntaxFactory.ParseExpression($"SetValue(value, nameof({property.Identifier.Text}))")))));

                                // Replace the old getter and setter with the new ones
                                var newProperty = property.WithInitializer(null) // Remove the initializer
                                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None)) // Remove the trailing semicolon
                                        .WithTrailingTrivia(property.GetTrailingTrivia()) // Preserve any trailing trivia
                                        .WithAccessorList(
                                SyntaxFactory.AccessorList(
                                        SyntaxFactory.List([newGetter, newSetter])));

                                // Replace the old property with the new one in the class
                                var newClassDeclaration = updatedClass.ReplaceNode(property, newProperty);

                                // Replace the old class declaration with the new one
                                root = root.ReplaceNode(classNode, newClassDeclaration);
                                break;
                            }
                        }
                    }
                }

                //// Format the modified syntax tree to make it pretty
                var formattedRoot = Formatter.Format(root, new AdhocWorkspace());

                //// Get the new code as a string
                var newCode = formattedRoot.ToFullString();

                // Get the current text buffer and cursor position
                var textBuffer = docView.TextBuffer;
                var caretPosition = docView.TextView.Caret.Position.BufferPosition;

                // Start a text edit operation
                using (var edit = textBuffer.CreateEdit())
                {
                    // Replace the entire text with the new code
                    edit.Replace(new Span(0, textBuffer.CurrentSnapshot.Length), newCode);

                    // Apply the edit
                    edit.Apply();
                }

                // Restore the cursor position
                docView.TextView.Caret.MoveTo(caretPosition);
            }
        }
    }
}
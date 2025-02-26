#if WINDOWS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace SharpBIM.GitTracker.Core.WPF.Helpers
{
    public static class GitHelper
    {
        public static string? GetGitRepositoryPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE2 dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            if (dte?.Solution == null || string.IsNullOrEmpty(dte.Solution.FullName))
            {
                return null;
            }

            string solutionDir = Path.GetDirectoryName(dte.Solution.FullName);
            if (solutionDir == null)
                return null;

            return solutionDir;
        }

        public static string? FindGitRepositoryPath(string startDirectory)
        {
            string? directory = startDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                string gitPath = Path.Combine(directory, ".git");

                if (Directory.Exists(gitPath) || File.Exists(gitPath))  // .git can be a file (worktrees)
                {
                    return directory;  // Found Git repository root
                }

                directory = Directory.GetParent(directory)?.FullName;
            }

            return null;  // Not found
        }

        public static string? GetCurrentBranch(string repoPath)
        {
            string headFile = Path.Combine(repoPath, ".git", "HEAD");
            if (File.Exists(headFile))
            {
                string content = File.ReadAllText(headFile).Trim();
                if (content.StartsWith("ref: refs/heads/"))
                {
                    return content.Replace("ref: refs/heads/", ""); // Extract branch name
                }
                return content; // Detached HEAD state (commit hash)
            }

            return null;
        }

        public static string? GetRepoName(string repoPath)
        {
            string repoLink = null;
            if (repoPath == null)
                return string.Empty;
            string headFile = Path.Combine(repoPath, ".git", "config");
            if (File.Exists(headFile))
            {
                var lines = File.ReadAllLines(headFile);
                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("url = "))
                    {
                        repoLink = line.Replace("url = ", "").Split('/').Last().Replace(".git", ""); // Extract Repo name
                        break;
                    }
                }
            }
            return repoLink;
        }
    }
}

#endif
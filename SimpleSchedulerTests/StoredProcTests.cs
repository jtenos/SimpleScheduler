using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace SimpleSchedulerTests;

[TestClass]
public class StoredProcTests
{
	[TestMethod]
	public void TestProcedureFormat()
    {
		DirectoryInfo currentDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!;
		while (!currentDir.GetFiles("SimpleScheduler.sln").Any())
		{
			currentDir = new(Path.Combine(currentDir.FullName, ".."));
			currentDir.Refresh();
		}

		currentDir = new(Path.Combine(currentDir.FullName, 
			"SimpleSchedulerSqlServerDB", "app", "Procedures"));

		foreach (FileInfo procFile in currentDir.GetFiles("*.sql"))
        {
			int idx = 0;
			void AssertAreEqual(string expected, string actual)
			{
				Assert.AreEqual(expected, actual, $"File: {procFile.Name} - Line {idx}");
			}

			string[] lines = File.ReadAllLines(procFile.FullName);
			AssertAreEqual($"CREATE PROCEDURE [app].[{Path.GetFileNameWithoutExtension(procFile.Name)}]", lines[idx++]);
			while (lines[idx++] != "AS") { }
			AssertAreEqual("BEGIN", lines[idx++]);
			AssertAreEqual("\tSET TRANSACTION ISOLATION LEVEL SNAPSHOT;", lines[idx++]);
			AssertAreEqual("\tSET XACT_ABORT, NOCOUNT ON;", lines[idx++]);
			AssertAreEqual("", lines[idx++]);
			AssertAreEqual("\tBEGIN TRY", lines[idx++]);
			AssertAreEqual("\t\tBEGIN TRANSACTION;", lines[idx++]);
			AssertAreEqual("", lines[idx++]);
			while (lines[idx++] != "\t\tCOMMIT TRANSACTION;") { }
			AssertAreEqual("\tEND TRY", lines[idx++]);
			AssertAreEqual("\tBEGIN CATCH", lines[idx++]);
			AssertAreEqual("\t\tIF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;", lines[idx++]);
			AssertAreEqual("\t\tDECLARE @Msg NVARCHAR(2048) = ERROR_MESSAGE();", lines[idx++]);
			AssertAreEqual("\t\tRAISERROR(@Msg, 16, 1);", lines[idx++]);
			AssertAreEqual("\t\tRETURN 55555;", lines[idx++]);
			AssertAreEqual("\tEND CATCH;", lines[idx++]);
			AssertAreEqual("END;", lines[idx++]);
			AssertAreEqual("GO", lines[idx++]);
		}
	}
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

// The production code uses a local SQLite DB file (holdings.db) by default.
// Explicitly disabling test parallelization avoids file-level contention and
// satisfies MSTest analyzer MSTEST0001.
[assembly: DoNotParallelize]

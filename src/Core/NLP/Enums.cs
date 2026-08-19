namespace KitchenPC.Core.NLP;

public enum MatchPrecision
{
   None = 0,
   Partial = 1,
   Exact = 2,
};

public enum MatchResult
{
   NoMatch = 0,
   UnknownUnit = 1,
   NoForm = 2,
   UnknownForm = 3,
   IncompatibleForm = 4,
   PartialMatch = 5,
   Match = 6,
}

public enum AnomalousResult
{
   Fallthrough = 0,
   AutoConvert = 1,
}

public enum TraceLevel
{
   Debug = 0,
   Error = 1,
   Info = 2,
   Fatal = 3,
   Warn = 4,
}

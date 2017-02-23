using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace SolverCore
{
    public static class SolverMgr
    {
        private const string CurrentVersion = "1";
        private enum SolverTypes
        {
            Invalid,
            Solver2,
        }

        private static Dictionary<string, SolverTypes> SolverMap = new Dictionary<string, SolverMgr.SolverTypes>()
        {
            { typeof(Solver2.SolverData).FullName, SolverTypes.Solver2 }
        };

        public class SolverWrapper
        {
            public string TracingId { get; set; }
            public string SolverName { get; set; }
            public string SolutionSetId { get; set; }
            public string SolutionId { get; set; }
            public string Version { get; set; } = CurrentVersion;
            public string SolutionData { get; set; }
        }

        public class SolutionRunner
        {
            public SolverWrapper Wrapper { get; set; }
            public Func<CancellationToken, bool> Solver { get; set; }
        }

        public static string GetSolverWrapper(Solver2.SolverData solverData, string tracingId)
        {
            return JsonConvert.SerializeObject(new SolverWrapper()
            {
                TracingId = tracingId,
                SolverName = typeof(Solver2.SolverData).FullName,
                SolutionData = JsonConvert.SerializeObject(solverData),
                SolutionSetId = solverData.SolverCount.ToString(),
                SolutionId = solverData.SolutionIndex.ToString(),
            });
        }

        public static SolutionRunner GetSolver(string wrapperString, FlowBoard board)
        {
            var wrapper = JsonConvert.DeserializeObject<SolverWrapper>(wrapperString);

            if (wrapper.Version != CurrentVersion)
            {
                throw new ArgumentException($"Invalid Version {wrapper.Version} != {CurrentVersion}");
            }

            if (!SolverMap.ContainsKey(wrapper.SolverName))
            {
                string keys = "(" + string.Join(" ", SolverMap.Keys) + ")";
                throw new ArgumentException($"Unexpected {nameof(wrapper.SolverName)} value is {wrapper.SolverName} must be one of {keys}");
            }

            SolutionRunner retv = new SolutionRunner()
            {
                Wrapper = wrapper,
            };

            switch (SolverMap[wrapper.SolverName])
            {
                case SolverTypes.Solver2:
                    retv.Solver = GetSolver2Func(wrapper.SolutionData, board);
                    break;

                default:
                    Debug.Fail("Key found, no appropriate case found");
                    throw new ArgumentException($"Unexpected {nameof(wrapper.SolverName)}");
            }

            return retv;
        }

        private static Func<CancellationToken, Task<bool>> GetSolver2FuncAsync(string solutionData, FlowBoard board)
        {
            var solverData = JsonConvert.DeserializeObject<Solver2.SolverData>(solutionData);

            return new Func<CancellationToken, Task<bool>>(ct =>
            {
                board.InitializeBoard(solverData.BoardDefinition);
                return Solver2.SolveIterationAsync(solverData, board, ct);
            });
        }

        private static Func<CancellationToken, bool> GetSolver2Func(string solutionData, FlowBoard board)
        {
            var solverData = JsonConvert.DeserializeObject<Solver2.SolverData>(solutionData);

            return new Func<CancellationToken, bool>(ct =>
            {
                board.InitializeBoard(solverData.BoardDefinition);
                return Solver2.SolveIteration(solverData, board, ct);
            });
        }
    }
}

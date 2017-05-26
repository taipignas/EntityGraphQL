using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using EntityQueryLanguage.DataApi.Parsing;


namespace EntityQueryLanguage.DataApi
{
    public class DataManager<TContextType> where TContextType : IDisposable
    {
        private readonly Func<TContextType> _newContextFunc;
        public DataManager(Func<TContextType> newContextFunc)
        {
            _newContextFunc = newContextFunc;
        }

        public IDictionary<string, object> Query(string dataQuery, ISchemaProvider schemaProvider, IMethodProvider methodProvider, IRelationHandler relationHandler = null)
        {
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            var allData = new ConcurrentDictionary<string, object>();

            try
            {
                var objectGraph = new DataApiCompiler(schemaProvider, methodProvider, relationHandler).Compile(dataQuery);

                Parallel.ForEach(objectGraph.Fields, node =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(node.Error))
                        {
                            System.Console.WriteLine(node.Error);
                            allData[node.Name] = node.Error;
                        }
                        else
                        {
                            // fetch the data
                            using (var ctx = CreateContextValue())
                            {
                                var data = node.AsLambda().Compile().DynamicInvoke(ctx);
                                allData[node.Name] = data;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        allData[node.Name] = new { eql_error = ex.Message };
                    }
                });
            }
            catch (Exception ex)
            {
                allData["error"] = ex.Message;
            }
            timer.Stop();
            allData["_debug"] = new { TotalMilliseconds = timer.ElapsedMilliseconds };

            return allData;
        }

        /// Returns a new instance of the context type
        private TContextType CreateContextValue()
        {
            return _newContextFunc();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace BadPr.Api.Controllers
{
    [ApiController]
    [Route("files")]
    public class ParseFileController : ControllerBase
    {
        [HttpPost("upload")]
        public string Upload([FromForm]IList<IFormFile> files)
        {
            var name = DateTime.Now.ToString("yyyymmddhhmm");
            var uploadDirectory = Path.GetTempPath() + "\\Uploads\\";

            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }
            
            var file = System.IO.File.OpenWrite(uploadDirectory + name);
            files[0].CopyTo(file);
            file.Close();
            return name;
        }
        
        [HttpPost("download")]
        public Stream Download(string name)
        {
            var file = Path.GetTempPath() + "\\Uploads\\" + name;
            
            return System.IO.File.OpenRead(file);
        }
        
        [HttpGet("import/{name}")]
        public async Task<int> ImportAsync(string name)
        {
            var reader = new StreamReader(Path.GetTempPath() + "\\Uploads\\" + name);
            var contents = reader.ReadToEnd();
            
            var connection = new MySqlConnection("Server=localhost;User=mysql;Database=covid;Username=root");
            await connection.OpenAsync();
            
            new MySqlCommand("TRUNCATE areas; TRUNCATE stat_record;", connection).ExecuteNonQuery();

            int row = 0;
            foreach (var line in contents.Split("\n"))
            {
                if (line.Split(",").Length != 8)
                {
                    continue;
                }
                
                var areaType = ExtractItem("areaType", line);
                var areaCode = ExtractItem("areaCode", line);
                var areaName = ExtractItem("areaName", line);
                var date = ExtractItem("date", line);
                var age = ExtractItem("age", line);
                var cases = ExtractItem("newCasesBySpecimenDate", line);

                if (age == "unassigned")
                {
                    continue;
                }
                
                string age_start = null;
                string age_end = null;
                
                if (age.Contains("+"))
                {
                    age_start = age.Substring(0, 2);
                }
                else
                {
                    var x = age.Split("_");
                    age_start = x[0];
                    age_end = x[1];
                }

                var q = $"INSERT INTO areas(areaType, areaCode, areaName) VALUES ('{areaType}', '{areaCode}', '{areaName}');";
                var cmd = new MySqlCommand(q, connection);
                cmd.ExecuteNonQueryAsync().Wait();

                q = $"INSERT INTO stat_record(area_id, date, age_start, age_end, cases) VALUES ({cmd.LastInsertedId}, '{date}', {age_start}, {age_end ?? "NULL"}, {cases});";
                cmd = new MySqlCommand(q, connection);
                cmd.ExecuteNonQuery();

                row++;
            }

            return row;
        }
        
        [HttpPost("results")]
        public async Task<object> GetResults(string name)
        {
            var connection = new MySqlConnection("Server=localhost;User=mysql;Database=covid;Username=root");
            connection.OpenAsync().Wait();
            var cmd = new MySqlCommand("select * from stat_record r inner join areas a on a.id = r.area_id;", connection);
            var reader = cmd.ExecuteReader();

            var items = new List<object>();
            
            while(reader.Read())
            {
                var areaName = reader.GetString(9);
                var cases = reader.GetInt32(5);
                
                items.Add((areaName, cases));
            }

            var areas = new List<string>();
            foreach (var item in items)
            {
                var (area, cases) = ((string, int)) item;
                
                if (!areas.Contains(area))
                {
                    
                    
                    areas.Add(area);
                }
            }

            var results = new List<object>();
            foreach (var area in areas)
            {
                var count = 0;
                foreach (var item in items)
                {
                    if ((((string, int)) item).Item1 == area)
                    {
                        count += (((string, int)) item).Item2;
                    }
                }

                results.Add(new { area, count });
            }

            return results;
        }

        public string ExtractItem(string columnName, string line)
        {
            var columnOrder = new List<string>
            {
                "areaType",
                "areaCode",
                "areaName",
                "date",
                "age",
                "newCasesBySpecimenDate",
                "newCasesBySpecimenDateRollingSum",
                "newCasesBySpecimenDateRollingRate"
            };

            var colId = 0; 
            for (var i = 0; i < columnOrder.Count; i++)
            {
                if (columnOrder[i] == columnName)
                {
                    colId = i;
                    break;
                }
            }

            var items = line.Split(",");
            return items[colId];
        }
    }
}
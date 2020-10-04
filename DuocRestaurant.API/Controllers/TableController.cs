﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Services;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace DuocRestaurant.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private ITableService tableService { get; set; }
        private RestaurantDatabaseSettings dbSettings { get; set; }

        public TableController(ITableService authService, IOptions<RestaurantDatabaseSettings> databaseContext)
        {
            this.tableService = authService;
            this.dbSettings = databaseContext.Value;
        }

        [HttpGet]
        [ActionName("GetAll")]
        [Route("[action]")]
        public IActionResult Get()
        {
            IActionResult result;

            try
            {
                result = Ok(this.tableService.Get(this.dbSettings).MapAll(this.dbSettings, true));
            }
            catch (Exception ex)
            {
                result = BadRequest(ex.Message);
            }

            return result;
        }

        [HttpGet]
        [ActionName("GetById")]
        [Route("[action]/{id:int}")]
        public IActionResult Get([FromRoute(Name = "id")] int tableId)
        {
            IActionResult result;

            try
            {
                result = Ok(this.tableService.Get(this.dbSettings, tableId).Map(this.dbSettings, true));
            }
            catch (Exception ex)
            {
                result = BadRequest(ex.Message);
            }

            return result;
        }

        [HttpPost]
        public IActionResult Post([FromBody]Table table)
        {
            IActionResult result;

            try
            {
                var tables = this.tableService.Get(this.dbSettings);
                if (tables.Any(x => x.Number.Equals(table.Number)))
                    throw new Exception($"Ya existe una mesa con el número: { table.Number }");

                result = Ok(this.tableService.Add(this.dbSettings, table).Map(this.dbSettings, true));
            }
            catch (Exception ex)
            {
                result = BadRequest(ex.Message);
            }

            return result;
        }

        [HttpPut("{id}")]
        public IActionResult Put([FromRoute(Name = "id")] int tableId, [FromBody]Table table)
        {
            IActionResult result;

            try
            {
                var tables = this.tableService.Get(this.dbSettings);
                if (tables.Any(x => x.Id != tableId && x.Number.Equals(table.Number)))
                    throw new Exception($"Ya existe una mesa con el número: { table.Number }");

                result = Ok(this.tableService.Edit(this.dbSettings, tableId, table).Map(this.dbSettings, true));
            }
            catch (Exception ex)
            {
                result = BadRequest(ex.Message);
            }

            return result;
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute(Name = "id")] int tableId)
        {
            IActionResult result;

            try
            {
                result = Ok(this.tableService.Delete(this.dbSettings, tableId));
            }
            catch (Exception ex)
            {
                result = BadRequest(ex.Message);
            }

            return result;
        }

        [ActionName("FilterBy")]
        [Route("[action]")]
        public IActionResult FilterBy([FromBody]JObject filters)
        {
            IActionResult result;

            try
            {
                var tables = this.tableService.Get(this.dbSettings);

                if (filters.ContainsKey("Capacity"))
                {
                    int capacity = Convert.ToInt32(filters.GetValue("Capacity").ToString());

                    tables = tables.Where(x => x.Capacity >= capacity).ToList();
                }

                if (filters.ContainsKey("Active"))
                {
                    bool active = Convert.ToBoolean(filters.GetValue("Active").ToString());

                    tables = tables.Where(x => x.Active == active).ToList();
                }

                if (filters.ContainsKey("InUse"))
                {
                    bool inUse = Convert.ToBoolean(filters.GetValue("InUse").ToString());

                    tables = tables.Where(x => x.InUse == inUse).ToList();
                }

                result = Ok(tables.MapAll(this.dbSettings, true));
            }
            catch (Exception ex)
            {
                result = BadRequest(ex.Message);
            }

            return result;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StarChart.Data;
using StarChart.Models;

namespace StarChart.Controllers
{
    [ApiController]
    [Route("")]
    public class CelestialObjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CelestialObjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:int}", Name = "GetById")]
        public IActionResult GetById(int id)
        {
            var co = _context.CelestialObjects.Find(id);

            if (co is null)
            {
                return NotFound();
            }

            var satellites = GetSatellites(co);
            co.Satellites = satellites;

            return Ok(co);
        }

        private List<CelestialObject> GetSatellites(CelestialObject celestialObject)
        {
            var satellites = _context.CelestialObjects
                .Where(o => o.OrbitedObjectId != null && o.OrbitedObjectId == celestialObject.Id)
                .ToList();

            return satellites;
        }

        [HttpGet("{name}", Name = "GetByName")]
        public IActionResult GetByName(string name)
        {
            var celestialObjects = _context.CelestialObjects.Where(o => o.Name.Equals(name)).ToList();

            if (!celestialObjects.Any())
                return NotFound();

            foreach (var celestialObject in celestialObjects)
            {
                var satellites = GetSatellites(celestialObject);
                celestialObject.Satellites = satellites;
            }

            return Ok(celestialObjects);
        }

        [HttpGet(Name = "GetAll")]
        public IActionResult GetAll()
        {
            var celestialObjects = new List<CelestialObject>();
            
            foreach (var celestialObject in _context.CelestialObjects)
            {
                var satellites = GetSatellites(celestialObject);
                celestialObject.Satellites = satellites;
                celestialObjects.Add(celestialObject);
            }

            return Ok(celestialObjects);
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] CelestialObject celestialObject)
        {
            
            if (_context.CelestialObjects.Contains(celestialObject))
                return Conflict();
            
            _context.CelestialObjects.Add(celestialObject);
            _context.SaveChanges();

            return CreatedAtRoute("GetById", new { celestialObject.Id }, celestialObject);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, CelestialObject newCelestialObject)
        {
            var celestialObject = _context.CelestialObjects.Find(id);

            if (celestialObject is null)
                return NotFound();

            UpdateCelestialObject(celestialObject, newCelestialObject);
            return NoContent();
        }

        [HttpPatch("{id}/{name}")]
        public IActionResult RenameObject(int id, string name)
        {
            var celestialObject = _context.CelestialObjects.Find(id);

            if (celestialObject is null)
                return NotFound();

            celestialObject.Name = name;
            _context.CelestialObjects.Update(celestialObject);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var celestialObjects = _context.CelestialObjects.Where(c => c.Id == id || (c.OrbitedObjectId != null && c.OrbitedObjectId == id)).ToList();

            if (!celestialObjects.Any())
                return NotFound();
            
            _context.CelestialObjects.RemoveRange(celestialObjects);
            _context.SaveChanges();

            return NoContent();
        }

        private void UpdateCelestialObject(CelestialObject celestialObject, CelestialObject newCelestialObject)
        {
            celestialObject.Name = newCelestialObject.Name;
            celestialObject.OrbitalPeriod = newCelestialObject.OrbitalPeriod;
            celestialObject.OrbitedObjectId = newCelestialObject.OrbitedObjectId;

            _context.CelestialObjects.Update(celestialObject);
            _context.SaveChanges();
        }
    }
}

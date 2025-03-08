using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WEB_API.Models;
using Xunit.Sdk;

namespace WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly ProdutosUsuariosContext _context;

        public ProdutosController(ProdutosUsuariosContext context)
        {
            _context = context;
        }

        // GET: api/Produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            return await _context.Produtos.ToListAsync();
        }

        // GET: api/Produtos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
            {
                return NotFound();
            }

            return produto;
        }

        // POST: api/Produtos
        [HttpPost]
        public async Task<ActionResult<Produto>> PostProduto(ProdutoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(406, ModelState);
            }

            try
            {
                var imagemBytes = Array.Empty<byte>();
                if (!string.IsNullOrEmpty(request.Imagem))
                {
                    imagemBytes = Convert.FromBase64String(request.Imagem);
                }

                var produto = Produto.CriarNovoProduto(request.Nome ?? "Produto sem Nome", request.Preco, imagemBytes);

                _context.Produtos.Add(produto);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetProduto", new { id = produto.Id }, produto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Produtos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, ProdutoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(406, ModelState);
            }

            if (id != request.Id)
            {
                return BadRequest("O ID do produto na rota não corresponde ao ID no corpo da requisição.");
            }

            try
            {
                var produtoExistente = await _context.Produtos.FindAsync(id);
                if (produtoExistente == null)
                {
                    return NotFound();
                }

                var imagemBytes = Array.Empty<byte>();
                if (!string.IsNullOrEmpty(request.Imagem))
                {
                    imagemBytes = Convert.FromBase64String(request.Imagem);
                }

                var produtoAtualizado = Produto.CriarProdutoExistente(id, request.Nome ?? "Produto sem Nome", request.Preco, imagemBytes);

                _context.Entry(produtoExistente).CurrentValues.SetValues(produtoAtualizado);
                _context.Entry(produtoExistente).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProdutoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Produtos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProdutoExists(int id)
        {
            return _context.Produtos.Any(e => e.Id == id);
        }

        public class ProdutoRequest
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "O nome do produto é obrigatório.")]
            [StringLength(255, MinimumLength = 3, ErrorMessage = "O nome do produto deve ter pelo menos 3 caracteres.")]
            public string? Nome { get; set; }

            [Range(0.50, double.MaxValue, ErrorMessage = "O preço do produto não pode ser inferior a 0,50.")]
            public double Preco { get; set; }

            public string? Imagem { get; set; } // Adicionado
        }
    }
}
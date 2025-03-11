using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using WEB_API.Controllers;
using WEB_API.Models;
using WEB_API.Services;
using Xunit;

namespace WEB_API.test.Controllers
{
    public class ProdutosControllerTests
    {
        [Fact]
        public async Task GetProdutos_RetornaListaDeProdutos()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ProdutosUsuariosContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ProdutosUsuariosContext(options))
            {
                context.Produtos.Add(new Produto { Id = 1, Nome = "Produto 1", Preco = 10.0 });
                context.Produtos.Add(new Produto { Id = 2, Nome = "Produto 2", Preco = 20.0 });
                context.SaveChanges();
            }

            using (var context = new ProdutosUsuariosContext(options))
            {
                var loggerMock = new Mock<ILogger<ProdutosController>>();
                var logErroServiceMock = new Mock<ILogErroService>();
                var controller = new ProdutosController(context, loggerMock.Object, logErroServiceMock.Object);

                // Act
                var result = await controller.GetProdutos();

                // Assert
                var actionResult = Assert.IsType<ActionResult<IEnumerable<Produto>>>(result);
                var produtos = Assert.IsType<List<Produto>>(actionResult.Value);
                Assert.Equal(2, produtos.Count);
            }
        }

        [Fact]
        public async Task PostProduto_ComDadosValidos_RetornaCreatedAtAction()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ProdutosUsuariosContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ProdutosUsuariosContext(options))
            {
                var loggerMock = new Mock<ILogger<ProdutosController>>();
                var logErroServiceMock = new Mock<ILogErroService>();
                var controller = new ProdutosController(context, loggerMock.Object, logErroServiceMock.Object);

                // Configurar o Request
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                // Gerar uma string base64 válida
                byte[] randomBytes = new byte[20];
                new Random().NextBytes(randomBytes);
                string base64String = Convert.ToBase64String(randomBytes);

                var request = new ProdutosController.ProdutoRequest
                {
                    Nome = "Novo Produto",
                    Preco = 15.0,
                    Imagem = base64String // Usar a string base64 válida
                };

                // Act
                try
                {
                    // Ponto de interrupção 1: Verificar a criação do produto
                    var produto = Produto.CriarNovoProduto(request.Nome, request.Preco, Convert.FromBase64String(request.Imagem));
                    System.Diagnostics.Debug.WriteLine($"Produto criado: {produto.Nome}, {produto.Preco}");

                    // Ponto de interrupção 2: Verificar a adição ao contexto
                    context.Produtos.Add(produto);
                    System.Diagnostics.Debug.WriteLine("Produto adicionado ao contexto");

                    // Ponto de interrupção 3: Verificar o salvamento no banco de dados
                    await context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("Alterações salvas no banco de dados");

                    // Ponto de interrupção 4: Verificar o retorno do CreatedAtAction
                    var result = await controller.PostProduto(request);
                    var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
                    var produtoRetornado = Assert.IsType<Produto>(actionResult.Value);
                    Assert.Equal("Novo Produto", produtoRetornado.Nome);
                    Assert.Equal(15.0, produtoRetornado.Preco);
                    Assert.NotNull(produtoRetornado.Imagem);
                    System.Diagnostics.Debug.WriteLine("CreatedAtAction retornado corretamente");
                }
                catch (Exception ex)
                {
                    // Depurar a exceção
                    System.Diagnostics.Debug.WriteLine($"Exceção capturada: {ex.ToString()}");
                    throw; // Lançar a exceção novamente para o teste falhar
                }
            }
        }

        [Fact]
        public async Task PostProduto_ComModelStateInvalido_RetornaStatusCode406()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ProdutosUsuariosContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ProdutosUsuariosContext(options))
            {
                var loggerMock = new Mock<ILogger<ProdutosController>>();
                var logErroServiceMock = new Mock<ILogErroService>();
                var controller = new ProdutosController(context, loggerMock.Object, logErroServiceMock.Object);

                controller.ModelState.AddModelError("Nome", "Nome é obrigatório"); // Simula um erro de validação

                var request = new ProdutosController.ProdutoRequest
                {
                    Nome = null, // Nome inválido
                    Preco = 15.0,
                    Imagem = "base64encodedimage"
                };

                // Act
                var result = await controller.PostProduto(request);

                // Assert
                var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
                Assert.Equal(406, statusCodeResult.StatusCode);
            }
        }

        [Fact]
        public async Task PostProduto_ComArgumentException_RetornaBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ProdutosUsuariosContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ProdutosUsuariosContext(options))
            {
                var loggerMock = new Mock<ILogger<ProdutosController>>();
                var logErroServiceMock = new Mock<ILogErroService>();
                var controller = new ProdutosController(context, loggerMock.Object, logErroServiceMock.Object);

                // Configurar o Request
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                var request = new ProdutosController.ProdutoRequest
                {
                    Nome = "", // Nome inválido para ArgumentException
                    Preco = 15.0,
                    Imagem = "base64encodedimage"
                };

                // Act
                var result = await controller.PostProduto(request);

                // Assert
                var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
            }
        }

        [Fact]
        public async Task PostProduto_ComExceptionGenerica_RetornaStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ProdutosUsuariosContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ProdutosUsuariosContext(options))
            {
                var loggerMock = new Mock<ILogger<ProdutosController>>();
                var logErroServiceMock = new Mock<ILogErroService>();
                var controller = new ProdutosController(context, loggerMock.Object, logErroServiceMock.Object);

                // Configurar o Request
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                // Simula uma exceção genérica ao salvar as alterações
                var mockContext = new Mock<ProdutosUsuariosContext>(options);
                mockContext.Setup(c => c.SaveChangesAsync(default)).ThrowsAsync(new Exception("Erro genérico"));

                var controllerWithMockContext = new ProdutosController(mockContext.Object, loggerMock.Object, logErroServiceMock.Object);

                // Configurar o Request para o controller com mockContext
                controllerWithMockContext.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                var request = new ProdutosController.ProdutoRequest
                {
                    Nome = "Novo Produto",
                    Preco = 15.0,
                    Imagem = "base64encodedimage"
                };

                // Act
                var result = await controllerWithMockContext.PostProduto(request);

                // Assert
                var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
                Assert.Equal(500, statusCodeResult.StatusCode);
            }
        }
    }
}
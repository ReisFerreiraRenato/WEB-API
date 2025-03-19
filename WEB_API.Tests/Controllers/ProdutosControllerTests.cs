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

namespace WEB_API.Tests.Controllers
{
    public class ProdutosControllerTests
    {
        // Métodos Auxiliares
        private static Produto CriarProduto(int id, string nome, double preco, byte[] imagem = null)
        {
            return new Produto
            {
                Id = id,
                Nome = nome,
                Preco = preco,
                Imagem = imagem ?? Array.Empty<byte>()
            };
        }

        private static ProdutosController.ProdutoRequest CriarProdutoRequest(int id, string nome, double preco, string? imagem = null)
        {
            return new ProdutosController.ProdutoRequest
            {
                Id = id,
                Nome = nome,
                Preco = preco,
                Imagem = imagem
            };
        }

        private static string GerarBase64StringAleatoria()
        {
            byte[] randomBytes = new byte[20];
            new Random().NextBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        [Fact]
        public async Task GetProdutos_RetornaListaDeProdutos()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ProdutosUsuariosContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ProdutosUsuariosContext(options))
            {
                context.Produtos.Add(CriarProduto(1, "Produto 1",10.0));
                context.Produtos.Add(CriarProduto(2, "Produto 2",20.0));
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

                var request = CriarProdutoRequest(1, "Novo Produto", 15.0, GerarBase64StringAleatoria());

                // Act
                try
                {
                    var produto = Produto.CriarNovoProduto(request.Nome, request.Preco, Convert.FromBase64String(request.Imagem));
                    System.Diagnostics.Debug.WriteLine($"Produto criado: {produto.Nome}, {produto.Preco}");

                    context.Produtos.Add(produto);
                    System.Diagnostics.Debug.WriteLine("Produto adicionado ao contexto");

                    await context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("Alterações salvas no banco de dados");

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
                    // Depurar e lançar a exceção
                    System.Diagnostics.Debug.WriteLine($"Exceção capturada: {ex}");
                    throw; 
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

                controller.ModelState.AddModelError("Nome", "Nome é obrigatório");

                var request = CriarProdutoRequest(1, null, 15.0, "base64encodedimage");

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

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                var request = CriarProdutoRequest(1, "", 15.0, "base64encodedimage");

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

                var request = CriarProdutoRequest(1, "Novo Produto", 15.0, "base64encodedimage");

                // Act
                var result = await controllerWithMockContext.PostProduto(request);

                // Assert
                var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
                Assert.Equal(500, statusCodeResult.StatusCode);
            }
        }

        [Fact]
        public async Task GetProduto_ComIdInvalido_RetornaNotFound()
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

                // Configurar o ControllerContext e HttpContext
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                // Act
                var result = await controller.GetProduto(999);

                // Assert
                Assert.IsType<NotFoundObjectResult>(result.Result);
            }
        }

        [Fact]
        public async Task GetProduto_ComIdZero_RetornaNotFound()
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

                // Configurar o ControllerContext e HttpContext
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                // Act
                var result = await controller.GetProduto(0);

                // Assert
                Assert.IsType<NotFoundObjectResult>(result.Result);
            }
        }

        [Fact]
        public async Task GetProduto_ComExceptionGenerica_RetornaStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ProdutosUsuariosContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            var mockContext = new Mock<ProdutosUsuariosContext>(options);
            var mockDbSet = new Mock<DbSet<Produto>>();

            mockDbSet.Setup(m => m.FindAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Erro genérico"));

            mockContext.Setup(c => c.Produtos).Returns(mockDbSet.Object);

            var loggerMock = new Mock<ILogger<ProdutosController>>();
            var logErroServiceMock = new Mock<ILogErroService>();
            var controller = new ProdutosController(mockContext.Object, loggerMock.Object, logErroServiceMock.Object);

            // Configurar o ControllerContext e HttpContext
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.GetProduto(1);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, ((ObjectResult)result.Result).StatusCode);
        }

        [Fact]
        public async Task PutProduto_ComIdRotaDiferenteCorpo_RetornaBadRequest()
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

                var request = CriarProdutoRequest(2, "Novo Nome", 15.0 );

                // Act
                var result = await controller.PutProduto(1, request);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        [Fact]
        public async Task PutProduto_ComProdutoNaoEncontrado_RetornaNotFound()
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

                var request = CriarProdutoRequest(999, "Novo Nome",15.0);

                // Act
                var result = await controller.PutProduto(999, request);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async Task PutProduto_ComDadosValidos_AtualizaProduto()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ProdutosUsuariosContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;



            using (var context = new ProdutosUsuariosContext(options))
            {
                context.Produtos.Add(CriarProduto(1, "Produto Original", 10.0));
                context.SaveChanges();
            }

            using (var context = new ProdutosUsuariosContext(options))
            {
                var loggerMock = new Mock<ILogger<ProdutosController>>();
                var logErroServiceMock = new Mock<ILogErroService>();
                var controller = new ProdutosController(context, loggerMock.Object, logErroServiceMock.Object);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                var request = CriarProdutoRequest(1, "Produto Atualizado", 20.0, GerarBase64StringAleatoria());

                // Act
                var result = await controller.PutProduto(1, request);

                // Debugging
                var produtoAntes = await context.Produtos.FindAsync(1);
                System.Diagnostics.Debug.WriteLine($"Produto antes: {produtoAntes?.Nome}");

                // Assert
                Assert.IsType<NoContentResult>(result);

                var produtoAtualizado = await context.Produtos.FindAsync(1);
                Assert.NotNull(produtoAtualizado);
                Assert.Equal("Produto Atualizado", produtoAtualizado.Nome);
                Assert.Equal(20.0, produtoAtualizado.Preco);
                System.Diagnostics.Debug.WriteLine($"Produto depois: {produtoAtualizado?.Nome}");
            }
        }
        [Fact]
        public async Task DeleteProduto_ComIdInvalido_RetornaNotFound()
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

                // Act
                var result = await controller.DeleteProduto(999);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }
        [Fact]
        public async Task DeleteProduto_ComExceptionGenerica_RetornaStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ProdutosUsuariosContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Criar um mock do contexto para simular uma exceção
            var mockContext = new Mock<ProdutosUsuariosContext>(options);
            var mockDbSet = new Mock<DbSet<Produto>>();

            // Configurar o mock DbSet para lançar uma exceção ao chamar FindAsync
            mockDbSet.Setup(m => m.FindAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Erro genérico ao buscar produto"));

            // Configurar o mock contexto para retornar o mock DbSet
            mockContext.Setup(c => c.Produtos).Returns(mockDbSet.Object);

            var loggerMock = new Mock<ILogger<ProdutosController>>();
            var logErroServiceMock = new Mock<ILogErroService>();
            var controller = new ProdutosController(mockContext.Object, loggerMock.Object, logErroServiceMock.Object);

            // Configurar o ControllerContext e HttpContext
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.DeleteProduto(1);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
        [Fact]
        public async Task PutProduto_ComDbUpdateConcurrencyException_RetornaNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ProdutosUsuariosContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ProdutosUsuariosContext(options))
            {
                // Não adicionamos nenhum produto ao contexto, simulando um produto inexistente

                var loggerMock = new Mock<ILogger<ProdutosController>>();
                var logErroServiceMock = new Mock<ILogErroService>();
                var controller = new ProdutosController(context, loggerMock.Object, logErroServiceMock.Object);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                var request = CriarProdutoRequest(1, "Produto Atualizado", 20.00, "base64encodedimage");  // ID que não existe no banco de dados

                // Act
                var result = await controller.PutProduto(1, request);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }
        [Fact]
        public async Task PutProduto_ComDbUpdateConcurrencyException_LancaExcecao()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ProdutosUsuariosContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new ProdutosUsuariosContext(options))
            {
                // Adicionar um produto ao contexto
                var produtoOriginal = CriarProduto(1, "Produto Original", 10.0);
                context.Produtos.Add(produtoOriginal);
                context.SaveChanges();

                var loggerMock = new Mock<ILogger<ProdutosController>>();
                var logErroServiceMock = new Mock<ILogErroService>();
                var controller = new ProdutosController(context, loggerMock.Object, logErroServiceMock.Object);

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                var request = CriarProdutoRequest(1, "Produto Atualizado", 20.0, GerarBase64StringAleatoria());

                // Simular a DbUpdateConcurrencyException
                using (var secondContext = new ProdutosUsuariosContext(options))
                {
                    // Remover o produto em outro contexto, simulando outra transação
                    var produtoParaRemover = await secondContext.Produtos.FindAsync(1);
                    secondContext.Produtos.Remove(produtoParaRemover);
                    await secondContext.SaveChangesAsync();
                }

                // Act & Assert
                await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => controller.PutProduto(1, request));
            }
        }
        [Fact]
        public async Task PutProduto_ComModelStateInvalido_RetornaStatusCode406()
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

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                // Simular ModelState inválido
                controller.ModelState.AddModelError("Nome", "Nome é obrigatório");

                var request = CriarProdutoRequest(1, "Produto Atualizado", 20.0, "base64encodedimage");

                // Act
                var result = await controller.PutProduto(1, request);

                // Assert
                var statusCodeResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal(406, statusCodeResult.StatusCode);
            }
        }
        [Fact]
        public async Task PostProduto_ComImagemNula_CriaProdutoComImagemNula()
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

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                var request = CriarProdutoRequest(1, "Produto com Imagem Nula", 30.0, null);

                // Act
                var result = await controller.PostProduto(request);

                // Assert

                // 1. Verificação Direta do Banco de Dados
                var produtoSalvo = await context.Produtos.FirstOrDefaultAsync(p => p.Nome == "Produto com Imagem Nula");
                Assert.NotNull(produtoSalvo);
                Assert.Empty(produtoSalvo.Imagem);

                // 2. Verificação do CreatedAtActionResult
                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
                var produtoCriado = Assert.IsType<Produto>(createdAtActionResult.Value);
                Assert.Empty(produtoCriado.Imagem);

                // 3. Verificação do ActionResult<Produto>.Result
                Assert.IsType<CreatedAtActionResult>(result.Result);
            }
        }
        [Fact]
        public async Task PostProduto_ComImagemVazia_CriaProdutoComImagemVazia()
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

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                var request = CriarProdutoRequest(1, "Produto com Imagem Vazia", 40.0, ""); // Imagem vazia

                // Act
                var result = await controller.PostProduto(request);

                // Assert
                // 1. Verificação Direta do Banco de Dados
                var produtoSalvo = await context.Produtos.FirstOrDefaultAsync(p => p.Nome == "Produto com Imagem Vazia");
                Assert.NotNull(produtoSalvo);
                Assert.Empty(produtoSalvo.Imagem);

                // 2. Verificação do CreatedAtActionResult
                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
                var produtoCriado = Assert.IsType<Produto>(createdAtActionResult.Value);
                Assert.Empty(produtoCriado.Imagem);

                // 3. Verificação do ActionResult<Produto>.Result
                Assert.IsType<CreatedAtActionResult>(result.Result);
            }
        }
        [Fact]
        public async Task GetProduto_ComIdValido_RetornaProdutoCorreto()
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

                var produto = CriarProduto(1, "Produto de Teste", 50.00, Array.Empty<byte>());

                context.Produtos.Add(produto);
                await context.SaveChangesAsync();

                // Act
                var result = await controller.GetProduto(produto.Id);

                // Assert
                var actionResult = Assert.IsType<ActionResult<Produto>>(result);
                var produtoRetornado = Assert.IsType<Produto>(actionResult.Value);
                Assert.Equal(produto.Id, produtoRetornado.Id);
                Assert.Equal(produto.Nome, produtoRetornado.Nome);
                Assert.Equal(produto.Preco, produtoRetornado.Preco);
                Assert.Equal(produto.Imagem, produtoRetornado.Imagem);
            }
        }
        [Fact]
        public async Task PostProduto_ComPrecoNegativo_RetornaBadRequest()
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

                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };

                // Criar um request com preço negativo
                var request = CriarProdutoRequest(1,"Produto Negativo", -10.0, GerarBase64StringAleatoria());

                // Act
                var result = await controller.PostProduto(request);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result.Result);
            }
        }
    }
}
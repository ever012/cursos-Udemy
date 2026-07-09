using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using BibliotecaAPI.Validaciones;

namespace BibliotecaAPITests.PruebasUnitarias.Validaciones
{
    [TestClass]
    public class PrimeraLetraMayusculaAttributePruebas
    {
        [TestMethod]
        [DataRow("")]
        [DataRow("     ")]
        [DataRow("          ")]
        [DataRow(null)]
        [DataRow("Felipe")]
        public void IsValid_RetornaExistoso_SiValueNoTieneLaPrimeraLetraMinuscula(string value)  //el metodo se llama IsValid
        {
            //Preparacion
            var PrimeraLetraMayusculasAttribute = new PrimeraLetraMayusculasAttribute();
            var validationContext = new ValidationContext(new object());
            //var value = string.Empty; //no es necesario porque ahora se usa DataRow


            //Prueba
            var resultado = PrimeraLetraMayusculasAttribute.GetValidationResult(value, validationContext);


            //Verificacion
            Assert.AreEqual(expected: ValidationResult.Success, actual: resultado);


        }

        [TestMethod]
        [DataRow("felipe")]
        public void IsValid_RetornaError_SiValueTieneLaPrimeraLetraMinuscula(string value)
        {
            //Preparacion
            var PrimeraLetraMayusculasAttribute = new PrimeraLetraMayusculasAttribute();
            var validationContext = new ValidationContext(new object());
            //var value = string.Empty; //no es necesario porque ahora se usa DataRow


            //Prueba
            var resultado = PrimeraLetraMayusculasAttribute.GetValidationResult(value, validationContext);


            //Verificacion
            Assert.AreEqual(expected: "La primera letra debe ser mayúscula", actual: resultado!.ErrorMessage);


        }


    }
}

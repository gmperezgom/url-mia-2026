using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Laboratorio_2_CSV_JSON;
class Program{
    static void Main(string [] args){
       //Definimos la ruta como string
       string ruta= "estudiantes.csv";
       //Guardamos todas las lineas en nuestra variable lineas
       string[] lineas= File.ReadAllLines(ruta);
       //Declaramos la lista estudiantes
       List<Estudiante> ListaEstudiantes= new List<Estudiante>();

       //Declaramos el ciclo for para el recorrido de cada uno de los estudiantes
       for(int i=1; i< lineas.Length; i++ ){
            //utilizamos la variable datos para partir en un arreglo de strings
            string[] datos= lineas[i].Split(',');
            //Creamos un objeto Estudiante nuevo
            Estudiante estudiante = new Estudiante{
                Id= int.Parse(datos[0]),
                Nombre= datos[1],
                Carrera= datos[2]
            };
            //agregamos el objeto estudiante creado a nuestra lista
            ListaEstudiantes.Add(estudiante);
       }  
       //Creamos un foreach para la listaEstudiantes para poder imprimir el estudiante con sus respectivos datos     
       foreach(var elemento in ListaEstudiantes){
            Console.WriteLine(elemento.Id+"-"+elemento.Nombre+"-"+elemento.Carrera);
       }
       //Convertimos la listaEstudiantes a un string en formato JSON
       var opciones= new JsonSerializerOptions{WriteIndented=true};
       string estudianteJSON= JsonSerializer.Serialize(ListaEstudiantes, opciones);
    
       //ahora definimos la ruta json en un string
       string rutaJSON= "estudiantes.json";
       //ahora creamos el archivo de texto con el contenido de los estudiantes
       File.WriteAllText(rutaJSON,estudianteJSON);

       //Confirmamos que el archivo se ha creado de forma correcta
       Console.WriteLine("Archivo estudiantes.json creado de forma exitosa");
    }

}
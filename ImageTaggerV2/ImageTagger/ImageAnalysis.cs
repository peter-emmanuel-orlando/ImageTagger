using Clarifai.API;
using Clarifai.DTOs.Inputs;
using ImageTagger;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ImageAnalysisAPI
{
    public static class ImageAnalysis
    {

        public enum SuggestionCategories
        {
            None = 0,
            Demographics,
            Apparel,
            Appropriate,



        }

        public struct TagAndCategory
        {
            public TagAndCategory(string tagName, string tagCategory) : this()
            {
                TagName = tagName;
                TagCategory = tagCategory;
            }

            public string TagName { get; }
            public string TagCategory { get; }
        }

        public static void GetSuggestions()
        {
            Task.Run(GetSuggestionsAsync);
        }

        private static async Task GetSuggestionsAsync()
        {
            var apiKey = SettingsPersistanceUtil.RetreiveSetting("apiKey");
            var client = new ClarifaiClient(apiKey);
            // When using async/await
            var bytes = File.ReadAllBytes(@"C:\Users\YumeMura\Downloads\New folder\Ashley Chanel.jpg");
            var res = await client.PublicModels.DemographicsModel
                .Predict(new ClarifaiFileImage(bytes))
                .ExecuteAsync();

            // Print the concepts
            foreach (var concept in res.Get().Data)
            {
                foreach (var innerConcept in concept.AgeAppearanceConcepts)
                    Debug.WriteLine($"{innerConcept.Name}: {innerConcept.Value}");
                foreach (var innerConcept in concept.GenderAppearanceConcepts)
                    Debug.WriteLine($"{innerConcept.Name}: {innerConcept.Value}");
                foreach (var innerConcept in concept.MulticulturalAppearanceConcepts)
                    Debug.WriteLine($"{innerConcept.Name}: {innerConcept.Value}");

                Debug.WriteLine($"TYPE: {concept.TYPE}");
            }
        }
    }

}

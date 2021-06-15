# Why TinyTools

I came accross this post on stackoverflow: [String interpolation using named parameters](https://stackoverflow.com/questions/36745164/string-interpolation-using-named-parameters-in-c6) and I have looked to solve this problem in the past, so I decided to start my tiny tools project to put all my tiny solutions into one project. Please leave a star if you like it.


<!-- GETTING STARTED -->
## Getting Started

This is an example of how you may give instructions on setting up your project locally.
To get a local copy up and running follow these simple example steps.

### Prerequisites



.net cli should be installed.



### Installation

1. Find the nuget package [https://nuget.org](https://www.nuget.org/packages/LowlandTech.TinyTools/)
2. Or clone the repo

   ```sh
   git clone https://github.com/wendellmva/tinytools.git
   ```

3. Install nuget packages

   ```sh
   dotnet add package LowlandTech.TinyTools
   ```

4. Or user visual studio to add the package




<!-- USAGE EXAMPLES -->
## Usage



```csharp
    var person = new Person
    {
        FirstName = "John",
        LastName = "Smith"
    };
    var template = "Hello world, I'm {FirstName} {LastName}";

    var result = template.Interpolate(person);
    result.ShouldBe("Hello world, I'm John Smith");
```



<!-- ROADMAP -->
## Roadmap

See the [open issues](https://github.com/wendellmva/tinytools/issues) for a list of proposed features (and known issues).



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to be learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request



<!-- LICENSE -->
## License


Distributed under the MIT License. See `LICENSE` for more information.



<!-- CONTACT -->
## Contact

Your Name - [@wendellmva](https://twitter.com/wendellmva) - wendellmva@lowlandtech.nl

Project Link: [https://github.com/wendellmva/tinytools](https://github.com/wendellmva/tinytools)



<!-- ACKNOWLEDGEMENTS -->
## Acknowledgements
* [Awesome assertion library Shouldly](https://docs.shouldly.io/)

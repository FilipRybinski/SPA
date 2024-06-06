def sort_and_remove_duplicates(input_file, output_file):
    # Wczytanie danych z pliku
    with open(input_file, 'r') as file:
        lines = file.readlines()
    
    # Podzielenie danych na zestawy
    datasets = []
    for i in range(0, len(lines), 3):
        datasets.append((lines[i].strip(), lines[i+1].strip(), lines[i+2].strip()))
    
    # Usunięcie zduplikowanych zestawów
    unique_datasets = list(set(datasets))
    
    # Funkcja do wyodrębnienia fragmentu linii od piątego wyrazu do końca
    def get_rest_from_fifth_word(description):
        words = description.split()
        if len(words) >= 5:
            return " ".join(words[4:])
        return ""
    
    # Sortowanie zestawów danych na podstawie fragmentu od piątego wyrazu do końca
    sorted_datasets = sorted(unique_datasets, key=lambda x: get_rest_from_fifth_word(x[1]))
    
    # Zapisanie posortowanych zestawów do nowego pliku
    with open(output_file, 'w') as file:
        for dataset in sorted_datasets:
            file.write(f"{dataset[0]}\n")
            file.write(f"{dataset[1]}\n")
            file.write(f"{dataset[2]}\n")
    
    print(f"Dane zostały posortowane i zapisane w pliku {output_file}")

# Przykład użycia funkcji
input_file = 'BasicQueries.txt'
output_file = 'Posortowane_testy.txt'
sort_and_remove_duplicates(input_file, output_file)
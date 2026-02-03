# Dokumentacja Projektu LickedIn

## 1. Przegląd Projektu
**LickedIn** to system zarządzania zasobami ludzkimi i projektami, którego głównym celem jest optymalne dopasowanie kompetencji pracowników do wymagań projektowych. Aplikacja pozwala na zarządzanie bazą pracowników, słownikiem umiejętności oraz tworzenie projektów ze zdefiniowanym zapotrzebowaniem na konkretne role i kompetencje (wakaty).

## 2. Architektura
Aplikacja jest zbudowana w oparciu o framework **ASP.NET Core MVC** (Model-View-Controller) w wersji .NET 9.0.
*   **ORM**: Entity Framework Core.
*   **Baza danych**: SQLite (rozwojowo).
*   **Widoki**: Razor Views (.cshtml) z wykorzystaniem Bootstrap.

## 3. Struktura Danych (Modele)

### Główne Encje
*   **Employee** (`Pracownik`): Reprezentuje osobę w firmie. Zawiera dane osobowe (Imię, Nazwisko, PESEL, Kontakt) oraz listę posiadanych kompetencji.
*   **SkillType** (`Rodzaj Umiejętności`): Słownik dostępnych w systemie umiejętności (np. "C#", "SQL", "Komunikacja").
*   **Competency** (`Kompetencja`): Tabela łącząca (Many-to-Many z atrybutem) Pracownika i Umiejętność. Określa **poziom** (1-10) danej umiejętności u pracownika.

### Moduł Projektowy (Zarządzanie Wakatami)
*   **Project** (`Projekt`): Definiuje przedsięwzięcie z datą startu, końca i kierownikiem (`ManagerId`).
*   **ProjectMember** (`Członek Zespołu / Wakat`): Reprezentuje konkretne stanowisko w projekcie.
    *   Może być **obsadzone** (posiada `EmployeeId`) lub **wolne** (`EmployeeId` jest `null`).
    *   Reprezentuje jeden "slot" w zespole.
*   **VacancySkill** (`Wymaganie Wakatu`): Określa, jakie umiejętności i na jakim poziomie są wymagane dla danego stanowiska (`ProjectMember`).

## 4. Kluczowe Kontrolery

### `EmployeeController`
Zarządza cyklem życia pracownika. Umożliwia dodawanie, edycję i usuwanie pracowników. Wyświetla profil pracownika wraz z jego kompetencjami.

### `CompetencyController`
Odpowiada za przypisywanie umiejętności do pracowników. Pozwala określić poziom zaawansowania (1-10).

### `ProjectController` (Core Logic)
Najbardziej złożony kontroler zawierający logikę biznesową tworzenia zespołów.
*   **Tworzenie Projektu (`Create`)**: Umożliwia zdefiniowanie projektu oraz listy potrzebnych ról (wraz z wymaganymi skillami). Podczas zapisu uruchamiany jest **algorytm dopasowania**, który próbuje automatycznie obsadzić wakaty najlepszymi dostępnymi kandydatami.
*   **Edycja Projektu (`Edit`)**: Pozwala na zmianę parametrów projektu, dodawanie/usuwanie wakatów oraz zmianę wymagań. Posiada walidację chroniącą przed usunięciem wakatów, które są aktualnie obsadzone.
*   **Uzupełnianie Wakatów (`FillVacancies`)**: Funkcja dostępna w szczegółach projektu. Uruchamia algorytm dopasowania tylko dla pustych slotów, ignorując już obsadzone miejsca.

## 5. Algorytm Dopasowania (Matching Logic)
System wykorzystuje algorytm "najmniejszego deficytu kompetencyjnego" (`Lowest Deficit`) do dobierania pracowników do wakatów.

1.  **Deficyt**: Dla każdej wymaganej umiejętności na wakacie obliczana jest różnica: `Wymagany Poziom - Rzeczywisty Poziom Kandydata`. Jeśli kandydat przewyższa wymagania, deficyt wynosi 0.
2.  **Suma**: Deficyty ze wszystkich wymaganych umiejętności są sumowane dla każdego kandydata.
3.  **Wybór**: Do wakatu przypisywany jest kandydat z najniższą sumą deficytu.
4.  **Stan Wakatu**:
    *   Jeśli kandydat zostanie znaleziony -> Wakat jest tworzony jako obsadzony.
    *   Jeśli brak kandydata lub kandydaci są zajęci -> Wakat jest tworzony jako **pusty** (oczekujący).

## 6. Walidacja
Aplikacja implementuje zarówno walidację po stronie klienta (jQuery Unobtrusive Validaton) jak i serwera.
*   Unikalność PESEL.
*   Logika dat: Data zakończenia projektu nie może być wcześniejsza niż data rozpoczęcia (Custom Validation w `ProjectCreateViewModel` i `ProjectEditViewModel`).
*   Blokada usuwania obsadzonych wakatów.

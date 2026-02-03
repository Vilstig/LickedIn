import { test, expect } from '@playwright/test';

test.use({
    storageState: 'auth.json'
});

test('test', async ({ page }) => {
    await page.goto('http://localhost:5176/');
    await page.getByRole('link', { name: ' Projekty' }).click();
    await page.getByRole('link', { name: 'Start' }).click();
    await page.getByRole('link', { name: 'Zarządzaj Projektami' }).click();
    await page.getByRole('link', { name: ' Utwórz nowy projekt' }).click();
    await page.getByRole('button', { name: ' Utwórz projekt i dobierz' }).click();
    await page.getByRole('textbox', { name: 'Nazwa Projektu' }).fill('nazwa');
    await page.getByRole('button', { name: ' Utwórz projekt i dobierz' }).click();
    await page.getByLabel('Kierownik').selectOption('1');
    await page.getByRole('textbox', { name: 'Data Końca' }).fill('2026-02-01');
    await page.getByRole('button', { name: ' Utwórz projekt i dobierz' }).click();
    await page.locator('select[name="TeamMembers[0].RequiredSkills[0].SkillTypeId"]').selectOption('1');
    await page.getByRole('button', { name: ' Utwórz projekt i dobierz' }).click();
    await page.getByRole('button', { name: ' Utwórz projekt i dobierz' }).click();
    await page.getByRole('textbox', { name: 'Data Końca' }).fill('2026-02-04');
    await page.getByRole('button', { name: ' Utwórz projekt i dobierz' }).click();
});
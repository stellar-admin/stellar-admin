// Fuzzy match scoring for `sel-command`, ported from cmdk's command-score
// (https://github.com/pacocoursey/cmdk, MIT). A continuous match scores 1; 0 means no match.

// A match at the start of the string or directly after the previous match.
const SCORE_CONTINUE_MATCH = 1;
// A match at the start of a word; space-separated words score slightly higher than words
// separated by slashes, brackets, hyphens and so on.
const SCORE_SPACE_WORD_JUMP = 0.9;
const SCORE_NON_SPACE_WORD_JUMP = 0.8;
// Any other match.
const SCORE_CHARACTER_JUMP = 0.17;
// Two transposed letters are significantly penalised.
const SCORE_TRANSPOSITION = 0.1;
// Decays slightly with each skipped character.
const PENALTY_SKIPPED = 0.999;
// An exact-case match beats a case-insensitive one by a small amount.
const PENALTY_CASE_MISMATCH = 0.9999;
// A string with more characters than were typed scores slightly lower.
const PENALTY_NOT_COMPLETE = 0.99;

const IS_GAP_REGEXP = /[\\/_+.#"@[({&]/;
const COUNT_GAPS_REGEXP = /[\\/_+.#"@[({&]/g;
const IS_SPACE_REGEXP = /[\s-]/;
const COUNT_SPACE_REGEXP = /[\s-]/g;

function scoreInner(
  text: string,
  search: string,
  lowerText: string,
  lowerSearch: string,
  textIndex: number,
  searchIndex: number,
  memo: Map<string, number>,
): number {
  if (searchIndex === search.length) {
    return textIndex === text.length ? SCORE_CONTINUE_MATCH : PENALTY_NOT_COMPLETE;
  }

  const memoKey = `${textIndex},${searchIndex}`;
  const memoized = memo.get(memoKey);
  if (memoized !== undefined) {
    return memoized;
  }

  const searchChar = lowerSearch.charAt(searchIndex);
  let index = lowerText.indexOf(searchChar, textIndex);
  let highScore = 0;

  while (index >= 0) {
    let score = scoreInner(text, search, lowerText, lowerSearch, index + 1, searchIndex + 1, memo);
    if (score > highScore) {
      if (index === textIndex) {
        score *= SCORE_CONTINUE_MATCH;
      } else if (IS_GAP_REGEXP.test(text.charAt(index - 1))) {
        score *= SCORE_NON_SPACE_WORD_JUMP;
        const wordBreaks = text.slice(textIndex, index - 1).match(COUNT_GAPS_REGEXP);
        if (wordBreaks && textIndex > 0) {
          score *= Math.pow(PENALTY_SKIPPED, wordBreaks.length);
        }
      } else if (IS_SPACE_REGEXP.test(text.charAt(index - 1))) {
        score *= SCORE_SPACE_WORD_JUMP;
        const spaceBreaks = text.slice(textIndex, index - 1).match(COUNT_SPACE_REGEXP);
        if (spaceBreaks && textIndex > 0) {
          score *= Math.pow(PENALTY_SKIPPED, spaceBreaks.length);
        }
      } else {
        score *= SCORE_CHARACTER_JUMP;
        if (textIndex > 0) {
          score *= Math.pow(PENALTY_SKIPPED, index - textIndex);
        }
      }

      if (text.charAt(index) !== search.charAt(searchIndex)) {
        score *= PENALTY_CASE_MISMATCH;
      }
    }

    // Allow transposed letters and duplicate letters in the search.
    if (
      (score < SCORE_TRANSPOSITION &&
        lowerText.charAt(index - 1) === lowerSearch.charAt(searchIndex + 1)) ||
      (lowerSearch.charAt(searchIndex + 1) === lowerSearch.charAt(searchIndex) &&
        lowerText.charAt(index - 1) !== lowerSearch.charAt(searchIndex))
    ) {
      const transposedScore = scoreInner(
        text,
        search,
        lowerText,
        lowerSearch,
        index + 1,
        searchIndex + 2,
        memo,
      );
      if (transposedScore * SCORE_TRANSPOSITION > score) {
        score = transposedScore * SCORE_TRANSPOSITION;
      }
    }

    if (score > highScore) {
      highScore = score;
    }

    index = lowerText.indexOf(searchChar, index + 1);
  }

  memo.set(memoKey, highScore);
  return highScore;
}

function formatInput(value: string) {
  // Treat all space characters and hyphens as equivalent.
  return value.toLowerCase().replace(COUNT_SPACE_REGEXP, " ");
}

/** Scores how well `search` fuzzy-matches `value` followed by `keywords`; 0 means no match. */
export function commandScore(value: string, search: string, keywords: string) {
  const text = keywords ? `${value} ${keywords}` : value;
  return scoreInner(text, search, formatInput(text), formatInput(search), 0, 0, new Map());
}

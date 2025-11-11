-- =====================================================
-- BACKFILL TRUST SCORE HISTORY FOR EXISTING USERS
-- =====================================================
-- Purpose: Create initial history entries for existing TrustScores
--          that don't have any history records yet.
--
-- This is a one-time script to populate history for users
-- who had trust scores before the history tracking feature.
--
-- Safe to run multiple times (idempotent)
-- =====================================================

USE BookingService;
GO

-- Step 1: Show existing trust scores
PRINT '=== EXISTING TRUST SCORES ==='
SELECT
    TrustScoreId,
    UserId,
    Score,
    OrderId AS LastRelatedOrderId,
    CreatedAt AS LastUpdated
FROM TrustScores
ORDER BY UserId;
GO

-- Step 2: Show existing history entries (if any)
PRINT '=== EXISTING HISTORY ENTRIES ==='
SELECT
    HistoryId,
    UserId,
    ChangeAmount,
    PreviousScore,
    NewScore,
    Reason,
    ChangeType,
    CreatedAt
FROM TrustScoreHistories
ORDER BY UserId, CreatedAt DESC;
GO

-- Step 3: Find TrustScores without ANY history entries
PRINT '=== TRUST SCORES WITHOUT HISTORY (Will be backfilled) ==='
SELECT
    ts.TrustScoreId,
    ts.UserId,
    ts.Score AS CurrentScore,
    ts.CreatedAt
FROM TrustScores ts
LEFT JOIN TrustScoreHistories tsh ON ts.UserId = tsh.UserId
WHERE tsh.HistoryId IS NULL
ORDER BY ts.UserId;
GO

-- Step 4: INSERT initial history entries for users without history
PRINT '=== BACKFILLING INITIAL HISTORY ENTRIES ==='

INSERT INTO TrustScoreHistories (
    UserId,
    OrderId,
    ChangeAmount,
    PreviousScore,
    NewScore,
    Reason,
    ChangeType,
    AdjustedByAdminId,
    CreatedAt
)
SELECT
    ts.UserId,
    ts.OrderId,                    -- The last related order
    0,                              -- ChangeAmount = 0 (no change, just recording initial)
    ts.Score,                       -- PreviousScore = CurrentScore
    ts.Score,                       -- NewScore = CurrentScore
    'Initial score from existing system data',  -- Reason
    'INITIAL',                      -- ChangeType
    NULL,                           -- Not adjusted by admin
    ts.CreatedAt                    -- Use the original TrustScore creation date
FROM TrustScores ts
WHERE NOT EXISTS (
    -- Only insert if user has NO history entries at all
    SELECT 1
    FROM TrustScoreHistories tsh
    WHERE tsh.UserId = ts.UserId
);

-- Show how many rows were inserted
DECLARE @RowCount INT = @@ROWCOUNT;
PRINT CONCAT('✓ Inserted ', @RowCount, ' initial history entries');
GO

-- Step 5: Verify the backfill worked
PRINT '=== VERIFICATION: ALL HISTORY ENTRIES AFTER BACKFILL ==='
SELECT
    HistoryId,
    UserId,
    OrderId,
    ChangeAmount,
    PreviousScore,
    NewScore,
    Reason,
    ChangeType,
    CreatedAt,
    CASE
        WHEN AdjustedByAdminId IS NOT NULL THEN 'Admin Adjusted'
        ELSE 'System Generated'
    END AS Source
FROM TrustScoreHistories
ORDER BY UserId, CreatedAt DESC;
GO

PRINT '=== BACKFILL COMPLETE ==='
PRINT 'All existing trust scores now have initial history entries.'
PRINT 'New score changes will continue to add history entries automatically.'
GO
